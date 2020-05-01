import * as pulumi from '@pulumi/pulumi'
import * as docker from '@pulumi/docker'
import * as k8s from '@pulumi/kubernetes'
import * as path from 'path'
import * as config from '../../config'
import { k8sProvider } from '../cluster'
import { appsNamespaceName } from '../infrastructure/namespace'

class WebApp extends pulumi.ComponentResource {
    endpoint: pulumi.Output<string>

    constructor(
            name: string,
            namespace: pulumi.Output<string>,
            opts: pulumi.ComponentResourceOptions) {
        super('apps:WebApp', name, {}, opts)

        let runtimeImage = config.k8sConfig.usePi ?
            'arm32v7/nginx:1.17' :
            'nginx:1.17-alpine'

        const webAppImage = new docker.Image('fable-web-app', {
            imageName: `${config.dockerRegistry.server}/ameier38/fable-web-app`,
            build: {
                context: path.join(config.root, 'fable-web-app'),
                dockerfile: path.join(config.root, 'fable-web-app', 'deploy', 'Dockerfile'),
                args: { 
                    RUNTIME_IMAGE: runtimeImage,
                    FABLE_APP_SCHEME: 'https',
                    FABLE_APP_HOST: config.dnsConfig.tld,
                    FABLE_APP_PORT: '8080',
                    FABLE_APP_GRAPHQL_SCHEME: 'https',
                    FABLE_APP_GRAPHQL_HOST: `graphql.${config.dnsConfig.tld}`,
                    FABLE_APP_GRAPHQL_PORT: '80',
                    FABLE_APP_DISQUS_SHORTNAME: 'andrewmeier-dev'
                }
            },
            registry: config.dockerRegistry
        }, { parent: this })

        const webAppChart = new k8s.helm.v3.Chart('web-app', {
            path: path.join(config.root, 'infrastructure', 'charts', 'base-service'),
            namespace: namespace,
            values: {
                fullnameOverride: 'web-app',
                isGrpc: false,
                image: webAppImage.imageName
            }
        }, { parent: this })

        const serviceHost =
            pulumi.all([webAppChart, namespace])
            .apply(([chart, ns]) => chart.getResourceProperty('v1/Service', ns, 'web-app', 'metadata'))
            .apply(meta => `${meta.name}.${meta.namespace}.svc.cluster.local`)

        const servicePort =
            pulumi.all([webAppChart, namespace])
            .apply(([chart, ns]) => chart.getResourceProperty('v1/Service', ns, 'web-app', 'spec'))
            .apply(spec => spec.ports.find(port => port.name === 'http')?.port)

        this.endpoint = pulumi.interpolate `${serviceHost}:${servicePort}`

        this.registerOutputs({
            endpoint: this.endpoint
        })
    }
}

export const webApp = new WebApp(
    'web-app',
    appsNamespaceName,
    { provider: k8sProvider })
