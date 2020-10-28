import * as cloudflare from '@pulumi/cloudflare'
import * as docker from '@pulumi/docker'
import * as k8s from '@pulumi/kubernetes'
import * as pulumi from '@pulumi/pulumi'
import * as path from 'path'
import * as config from './config'
import { blogNamespace } from './namespace'

type GraphqlApiArgs = {
    tldZoneId: pulumi.Input<string>
    subdomain: pulumi.Input<string>
    loadBalancerAddress: pulumi.Input<string>
    acmeEmail: pulumi.Input<string>
    namespace: pulumi.Input<string>
    registryEndpoint: pulumi.Input<string>
    imageRegistry: pulumi.Input<docker.ImageRegistry>
    dockerCredentials: pulumi.Input<string>
    airtableBaseId: pulumi.Input<string>
    airtableApiKey: pulumi.Input<string>
}

class GraphqlApi extends pulumi.ComponentResource {
    host: pulumi.Output<string>
    internalHost: pulumi.Output<string>
    internalPort: pulumi.Output<number>

    constructor(name:string, args: GraphqlApiArgs, opts:pulumi.CustomResourceOptions) {
        super('blog:GraphqlApi', name, {}, opts)

        const record = new cloudflare.Record(`${name}-graphql-api`, {
            zoneId: args.tldZoneId,
            name: args.subdomain,
            type: 'A',
            value: args.loadBalancerAddress
        }, { parent: this })

        this.host = record.hostname

        const registrySecret = new k8s.core.v1.Secret(`${name}-graphql-api-registry`, {
            metadata: { namespace: args.namespace },
            type: 'kubernetes.io/dockerconfigjson',
            stringData: {
                '.dockerconfigjson': args.dockerCredentials
            }
        }, { parent: this })

        let airtableSecret = new k8s.core.v1.Secret(`${name}-airtable`, {
            metadata: { namespace: args.namespace },
            immutable: true,
            stringData: {
                'base-id': args.airtableBaseId,
                'api-key': args.airtableApiKey
            }
        }, { parent: this })

        let graphqlImage = new docker.Image(`${name}-graphql-api`, {
            imageName: pulumi.interpolate `${args.registryEndpoint}/blog/graphql-api`,
            build: path.join(config.root, 'graphql-api'),
            registry: args.imageRegistry
        }, { parent: this })

        const chartName = `${name}-graphql-api`
        const graphqlChart = new k8s.helm.v3.Chart(name, {
            chart: 'base-service',
            version: '0.1.2',
            fetchOpts: {
                repo: 'https://ameier38.github.io/charts'
            },
            namespace: args.namespace,
            values: {
                fullnameOverride: chartName,
                backendType: 'http',
                containerPort: 8080,
                image: graphqlImage.imageName,
                imagePullPolicy: 'Always',
                imagePullSecrets: [ registrySecret.metadata.name ],
                env: { 
                    DEBUG: "false",
                    AIRTABLE_SECRET: airtableSecret.metadata.name,
                    SERVER_PORT: 8080,
                },
                secrets: [ airtableSecret.metadata.name ]
            }
        }, { parent: this })

        this.internalHost =
            pulumi.all([graphqlChart, args.namespace])
            .apply(([chart, namespace]) => chart.getResourceProperty('v1/Service', namespace, chartName, 'metadata'))
            .apply(meta => `${meta.name}.${meta.namespace}.svc.cluster.local`)

        this.internalPort =
            pulumi.all([graphqlChart, args.namespace])
            .apply(([chart, namespace]) => chart.getResourceProperty('v1/Service', namespace, chartName, 'spec'))
            .apply(spec => spec.ports.find(port => port.name === 'http')!.port)

        // NB: generates certificate
        new k8s.apiextensions.CustomResource(`${name}-graphql-api`, {
            apiVersion: 'getambassador.io/v2',
            kind: 'Host',
            metadata: { namespace: args.namespace },
            spec: {
                hostname: this.host,
                acmeProvider: {
                    email: args.acmeEmail
                }
            }
        }, { parent: this })

        // NB: specifies how to direct incoming requests
        new k8s.apiextensions.CustomResource(`${name}-graphql-api`, {
            apiVersion: 'getambassador.io/v2',
            kind: 'Mapping',
            metadata: { namespace: args.namespace },
            spec: {
                prefix: '/',
                host: this.host,
                service: pulumi.interpolate `${this.internalHost}:${this.internalPort}`
            }
        }, { parent: this })

        this.registerOutputs({
            internalHost: this.internalHost,
            internalPort: this.internalPort,
            host: this.host
        })
    }
}

export const graphqlApi = new GraphqlApi(config.env, {
    namespace: blogNamespace.metadata.name,
    tldZoneId: config.tldZoneId,
    subdomain: 'graphql',
    acmeEmail: config.acmeEmail,
    registryEndpoint: config.registryEndpoint,
    imageRegistry: config.imageRegistry,
    dockerCredentials: config.dockerCredentials,
    loadBalancerAddress: config.loadBalancerAddress,
    airtableApiKey: config.airtableConfig.apiKey,
    airtableBaseId: config.airtableConfig.baseId
}, { provider: config.k8sProvider })
