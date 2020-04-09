import * as pulumi from '@pulumi/pulumi'
import * as docker from '@pulumi/docker'
import * as kx from '@pulumi/kubernetesx'
import * as path from 'path'
import { appsNamespaceName } from '../infrastructure/namespace'
import * as config from '../../config'

class GraphQL extends pulumi.ComponentResource {
    endpoint: pulumi.Output<string>

    constructor(
            name:string,
            namespace: pulumi.Output<string>,
            opts:pulumi.CustomResourceOptions) {
        super('apps:GraphqlApi', name, opts)

        let airtableSecret = new kx.Secret('airtable', {
            metadata: {
                namespace: namespace
            },
            stringData: {
                'url': config.airtableConfig.url,
                'api-key': config.airtableConfig.apiKey
            }
        }, { parent: this })

        let graphqlImage = new docker.Image('graphql-api', {
            imageName: `${config.dockerRegistry.server}/ameier38/graphql-api`,
            build: {
                context: path.join(config.root, 'graphql-api'),
                dockerfile: path.join(config.root, 'graphql-api', 'deploy', 'Dockerfile'),
                args: {
                    RUNTIME: 'linux-arm',
                    RUNTIME_IMAGE: 'mcr.microsoft.com/dotnet/core/runtime-deps:3.1.3-buster-slim-arm32v7'
                }
            },
            registry: config.dockerRegistry
        }, { parent: this })

        const podBuilder = new kx.PodBuilder({
            containers: [
                {
                    name: 'graphql-api',
                    image: graphqlImage.imageName,
                    imagePullPolicy: 'Always',
                    ports: { http: 4000 },
                    volumeMounts: [airtableSecret.mount('/var/secrets/airtable')]
                }
            ]
        })

        const deployment = new kx.Deployment('graphql-api', {
            metadata: {
                namespace: namespace
            },
            spec: podBuilder.asDeploymentSpec()
        }, { parent: this })

        const service = deployment.createService({
            type: kx.types.ServiceType.ClusterIP
        })

        const serviceName = service.metadata.name
        const servicePort = service.spec.ports.apply(ports => ports.find(port => port.name === 'http')!.port)
        this.endpoint = pulumi.interpolate `${serviceName}.${namespace}.svc.cluster.local:${servicePort}`

        this.registerOutputs({
            endpoint: this.endpoint
        })
    }
}

export const graphqlApi = new GraphQL(
    'graphql-api',
    appsNamespaceName,
    { provider: config.k8sProvider })
