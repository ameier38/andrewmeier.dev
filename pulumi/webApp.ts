import * as pulumi from '@pulumi/pulumi'
import * as docker from '@pulumi/docker'
import * as k8s from '@pulumi/kubernetes'
import * as path from 'path'
import * as config from './config'
import { blogNamespace } from './namespace'
import { graphqlApi } from './graphqlApi'

type WebAppArgs = {
    namespace: pulumi.Input<string>
    registryEndpoint: pulumi.Input<string>
    imageRegistry: pulumi.Input<docker.ImageRegistry>
    dockerCredentials: pulumi.Input<string>
    tld: pulumi.Input<string>
    graphqlHost: pulumi.Input<string>
}

class WebApp extends pulumi.ComponentResource {
    internalHost: pulumi.Output<string>
    internalPort: pulumi.Output<number>

    constructor(name: string, args: WebAppArgs, opts: pulumi.ComponentResourceOptions) {
        super('blog:WebApp', name, {}, opts)

        const registrySecret = new k8s.core.v1.Secret(`${name}-graphql-api-registry`, {
            metadata: { namespace: args.namespace },
            type: 'kubernetes.io/dockerconfigjson',
            stringData: {
                '.dockerconfigjson': args.dockerCredentials
            }
        }, { parent: this })

        const image = new docker.Image(`${name}-web-app`, {
            imageName: pulumi.interpolate `${args.registryEndpoint}/blog/web-app`,
            build: {
                context: path.join(config.root, 'web-app'),
                args: { 
                    RUNTIME_IMAGE: 'nginx:1.17-alpine',
                    APP_SCHEME: 'https',
                    APP_HOST: args.tld,
                    APP_PORT: '80',
                    GRAPHQL_SCHEME: 'https',
                    GRAPHQL_HOST: args.graphqlHost,
                    GRAPHQL_PORT: '80',
                    DISQUS_SHORTNAME: 'andrewmeier-dev'
                }
            },
            registry: args.imageRegistry
        }, { parent: this })

        const chartName = `${name}-web-app`
        const webAppChart = new k8s.helm.v3.Chart(name, {
            chart: 'base-service',
            version: '0.1.2',
            fetchOpts: {
                repo: 'https://ameier38.github.io/charts'
            },
            namespace: args.namespace,
            values: {
                fullnameOverride: chartName,
                imagePullPolicy: 'Always',
                imagePullSecrets: [ registrySecret.metadata.name ],
                backendType: 'http',
                containerPort: 8080,
                image: image.imageName
            }
        }, { parent: this })

        this.internalHost =
            pulumi.all([webAppChart, args.namespace])
            .apply(([chart, namespace]) => chart.getResourceProperty('v1/Service', namespace, chartName, 'metadata'))
            .apply(meta => `${meta.name}.${meta.namespace}.svc.cluster.local`)

        this.internalPort =
            pulumi.all([webAppChart, args.namespace])
            .apply(([chart, namespace]) => chart.getResourceProperty('v1/Service', namespace, chartName, 'spec'))
            .apply(spec => spec.ports.find(port => port.name === 'http')!.port)

        // NB: specifies how to direct incoming requests
        new k8s.apiextensions.CustomResource(`${name}-web-app`, {
            apiVersion: 'getambassador.io/v2',
            kind: 'Mapping',
            metadata: { namespace: args.namespace },
            spec: {
                prefix: '/',
                host: args.tld,
                service: pulumi.interpolate `${this.internalHost}:${this.internalPort}`
            }
        }, { parent: this })

        this.registerOutputs({
            internalHost: this.internalHost,
            internalPort: this.internalPort
        })
    }
}

export const webApp = new WebApp(config.env, {
    tld: config.tld,
    namespace: blogNamespace.metadata.name,
    registryEndpoint: config.registryEndpoint,
    imageRegistry: config.imageRegistry,
    dockerCredentials: config.dockerCredentials,
    graphqlHost: graphqlApi.host
}, { provider: config.k8sProvider })
