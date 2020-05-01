import * as pulumi from '@pulumi/pulumi'
import * as docker from '@pulumi/docker'
import * as k8s from '@pulumi/kubernetes'
import * as path from 'path'
import { k8sProvider } from '../cluster'
import { appsNamespaceName } from '../infrastructure/namespace'
import * as config from '../../config'

class GraphqlApi extends pulumi.ComponentResource {
    endpoint: pulumi.Output<string>

    constructor(
            name:string,
            namespace: pulumi.Output<string>,
            opts:pulumi.CustomResourceOptions) {
        super('apps:GraphqlApi', name, {}, opts)

        let airtableSecret = new k8s.core.v1.Secret('airtable', {
            metadata: {
                name: 'airtable',
                namespace: namespace
            },
            immutable: true,
            stringData: {
                'url': config.airtableConfig.url,
                'api-key': config.airtableConfig.apiKey
            }
        }, { parent: this })

        let runtime = config.k8sConfig.usePi ?
            'linux-arm' :
            'linux-musl-x64'

        let runtimeImage = config.k8sConfig.usePi ?
            'mcr.microsoft.com/dotnet/core/runtime-deps:3.1.3-buster-slim-arm32v7' :
            'mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine'

        let graphqlImage = new docker.Image('graphql-api', {
            imageName: `${config.dockerRegistry.server}/ameier38/graphql-api`,
            build: {
                context: path.join(config.root, 'graphql-api'),
                dockerfile: path.join(config.root, 'graphql-api', 'deploy', 'Dockerfile'),
                args: {
                    RUNTIME: runtime,
                    RUNTIME_IMAGE: runtimeImage
                }
            },
            registry: config.dockerRegistry
        }, { parent: this })

        const graphqlChart = new k8s.helm.v3.Chart('graphql', {
            path: path.join(config.root, 'infrastructure', 'charts', 'base-service'),
            namespace: namespace,
            values: {
                fullnameOverride: 'graphql',
                isGrpc: false,
                image: graphqlImage.imageName,
                imagePullPolicy: 'Always',
                env: { SERVER_PORT: 8080, DEBUG: "false" },
                secrets: [ airtableSecret.metadata.name ]
            }
        }, { parent: this })

        const serviceHost =
            pulumi.all([graphqlChart, namespace])
            .apply(([chart, ns]) => chart.getResourceProperty('v1/Service', ns, 'graphql', 'metadata'))
            .apply(meta => `${meta.name}.${meta.namespace}.svc.cluster.local`)

        const servicePort =
            pulumi.all([graphqlChart, namespace])
            .apply(([chart, ns]) => chart.getResourceProperty('v1/Service', ns, 'graphql', 'spec'))
            .apply(spec => spec.ports.find(port => port.name === 'http')?.port)

        this.endpoint = pulumi.interpolate `${serviceHost}:${servicePort}`

        this.registerOutputs({
            endpoint: this.endpoint
        })
    }
}

export const graphqlApi = new GraphqlApi(
    'graphql-api',
    appsNamespaceName,
    { provider: k8sProvider })
