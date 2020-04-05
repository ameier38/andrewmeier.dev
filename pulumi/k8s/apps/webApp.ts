import * as pulumi from '@pulumi/pulumi'
import * as k8s from '@pulumi/kubernetes'
import * as kx from '@pulumi/kubernetesx'
import * as docker from '@pulumi/docker'
import * as path from 'path'
import * as config from '../../config'
import { appsNamespaceName } from '../infrastructure/namespace'

class WebApp extends pulumi.ComponentResource {
    endpoint: pulumi.Output<string>

    constructor(
            name: string,
            namespace: pulumi.Output<string>,
            opts: pulumi.ComponentResourceOptions) {
        super('apps:WebApp', name, {}, opts)

        const webAppImage = new docker.Image('web-app', {
            imageName: `${config.dockerRegistry.server}/ameier38/web-app`,
            build: {
                context: path.join(config.root, 'web-app'),
                dockerfile: path.join(config.root, 'web-app', 'deploy', 'Dockerfile'),
                args: { 
                    RUNTIME_IMAGE: 'arm32v7/nginx:1.17',
                    REACT_APP_GRAPHQL_SCHEME: 'https',
                    REACT_APP_GRAPHQL_HOST: `graphql.${config.dnsConfig.tld}`,
                    REACT_APP_GRAPHQL_PORT: '80',
                    REACT_APP_SEGMENT_SOURCE: 'test'
                }
            },
            registry: config.dockerRegistry
        }, { parent: this })

        const podBuilder = new kx.PodBuilder({
            containers: [
                {
                    image: webAppImage.imageName,
                    imagePullPolicy: 'Always',
                    ports: { http: 8080 },
                }
            ],
        })

        const deployment = new kx.Deployment('web-app', {
            metadata: {
                name: 'web-app',
                namespace: namespace
            },
            spec: podBuilder.asDeploymentSpec()
        }, { parent: this })

        const service = deployment.createService({
            type: kx.types.ServiceType.ClusterIP,
        })

        const serviceName = service.metadata.name
        const servicePort = service.spec.ports.apply(ports => ports.find(port => port.name === 'http')!.port)
        this.endpoint = pulumi.interpolate `${serviceName}.${namespace}.svc.cluster.local:${servicePort}`

        const ingressRoute = new k8s.apiextensions.CustomResource('web-app', {
            apiVersion: 'traefik.containo.us/v1alpha1',
            kind: 'IngressRoute',
            metadata: {
                name: 'web-app',
                namespace: namespace
            },
            spec: {
                entryPoints: ['web'],
                routes: [
                    {
                        match: 'Host(`andrewmeier.dev`)',
                        kind: 'Rule',
                        services: [ { name: service.metadata.name, port: 8080 } ]
                    },
                ]
            }
        }, { parent: this })

        this.registerOutputs({
            endpoint: this.endpoint
        })
    }
}

export const webApp = new WebApp(
    'web-app',
    appsNamespaceName,
    { provider: config.k8sProvider })
