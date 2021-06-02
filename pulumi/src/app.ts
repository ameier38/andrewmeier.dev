import * as pulumi from '@pulumi/pulumi'
import * as docker from '@pulumi/docker'
import * as k8s from '@pulumi/kubernetes'
import * as path from 'path'
import * as config from './config'

const identifier = 'blog'

const registrySecret = new k8s.core.v1.Secret(`${identifier}-registry`, {
    metadata: { namespace: config.blogNamespace },
    type: 'kubernetes.io/dockerconfigjson',
    stringData: {
        '.dockerconfigjson': config.dockerCredentials
    }
}, { provider: config.k8sProvider })

let airtableSecret = new k8s.core.v1.Secret(`${identifier}-airtable`, {
    metadata: { namespace: config.blogNamespace },
    immutable: true,
    stringData: {
        'base-id': config.airtableConfig.baseId,
        'api-key': config.airtableConfig.apiKey
    }
}, { provider: config.k8sProvider })

const image = new docker.Image(identifier, {
    imageName: pulumi.interpolate `${config.registryEndpoint}/blog`,
    build: {
        context: path.join(config.root, 'app'),
        dockerfile: path.join(config.root, 'app', 'docker', 'app.Dockerfile'),
        args: { 
            RUNTIME_IMAGE_TAG: '5.0-focal-arm32v7',
            RUNTIME_ID: 'linux-arm'
        }
    },
    registry: config.imageRegistry
})

const chart = new k8s.helm.v3.Chart(identifier, {
    chart: 'base-service',
    version: '0.1.2',
    fetchOpts: {
        repo: 'https://ameier38.github.io/charts'
    },
    namespace: config.blogNamespace,
    values: {
        nameOverride: identifier,
        fullnameOverride: identifier,
        imagePullPolicy: 'Always',
        imagePullSecrets: [ registrySecret.metadata.name ],
        backendType: 'http',
        containerPort: 5000,
        image: image.imageName,
        env: {
            DEBUG: "false",
            AIRTABLE_SECRET: airtableSecret.metadata.name,
            SERVER_PORT: 5000,
            SEQ_HOST: config.seqHost,
            SEQ_PORT: config.seqPort
        },
        secrets: [
            airtableSecret.metadata.name
        ],
        nodeSelector: { 'kubernetes.io/arch': 'arm' }
    }
}, { provider: config.k8sProvider })

const internalHost =
    pulumi.all([chart, config.blogNamespace])
    .apply(([chart, namespace]) => chart.getResourceProperty('v1/Service', namespace, identifier, 'metadata'))
    .apply(meta => `${meta.name}.${meta.namespace}.svc.cluster.local`)

const internalPort =
    pulumi.all([chart, config.blogNamespace])
    .apply(([chart, namespace]) => chart.getResourceProperty('v1/Service', namespace, identifier, 'spec'))
    .apply(spec => spec.ports.find(port => port.name === 'http')!.port)

// NB: specifies how to direct incoming requests
new k8s.apiextensions.CustomResource('blog-mapping', {
    apiVersion: 'getambassador.io/v2',
    kind: 'Mapping',
    metadata: { namespace: config.blogNamespace },
    spec: {
        prefix: '/',
        host: config.zone,
        service: pulumi.interpolate `${internalHost}:${internalPort}`
    }
}, { provider: config.k8sProvider })
