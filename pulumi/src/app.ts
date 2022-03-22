import * as pulumi from '@pulumi/pulumi'
import * as docker from '@pulumi/docker'
import * as k8s from '@pulumi/kubernetes'
import * as path from 'path'
import * as config from './config'

const identifier = 'blog'

const image = new docker.Image(identifier, {
    imageName: pulumi.interpolate `${config.registryEndpoint}/${identifier}`,
    build: {
        context: path.join(config.root, 'app'),
        args: { 
            RUNTIME_IMAGE_TAG: '6.0-alpine-arm64v8',
            RUNTIME_ID: 'linux-arm64'
        },
        extraOptions: ['--quiet']
    },
    registry: config.imageRegistry
})

const registrySecret = new k8s.core.v1.Secret(`${identifier}-registry`, {
    metadata: { namespace: config.blogNamespace },
    type: 'kubernetes.io/dockerconfigjson',
    stringData: {
        '.dockerconfigjson': config.dockerCredentials
    }
})

let notionSecret = new k8s.core.v1.Secret(`${identifier}-notion`, {
    metadata: { namespace: config.blogNamespace },
    immutable: true,
    stringData: {
        'token': config.notionConfig.token
    }
})

const labels = { 'app.kubernetes.io/name': identifier }

const deployment = new k8s.apps.v1.Deployment(identifier, {
    metadata: {
        name: identifier,
        namespace: config.blogNamespace
    },
    spec: {
        replicas: 1,
        selector: { matchLabels: labels },
        template: {
            metadata: {
                labels: labels,
                annotations: {
                    'prometheus.io/scrape': 'true',
                    'prometheus.io/path': '/metrics',
                    'prometheus.io/port': '5000',
                }
            },
            spec: {
                imagePullSecrets: [{
                    name: registrySecret.metadata.name
                }],
                containers: [{
                    name: 'app',
                    image: image.imageName,
                    imagePullPolicy: 'IfNotPresent',
                    env: [
                        { name: 'SECRETS_DIR', value: '/var/secrets' },
                        { name: 'SERVER_PORT', value: '5000' },
                        { name: 'NOTION_DATABASE_ID', value: config.notionConfig.databaseId }
                    ],
                    volumeMounts: [{
                        name: 'notion',
                        mountPath: '/var/secrets/notion'
                    }],
                    livenessProbe: {
                        httpGet: {
                            path: '/healthz',
                            port: 5000
                        },
                        initialDelaySeconds: 5
                    },
                    readinessProbe: {
                        httpGet: {
                            path: '/healthz',
                            port: 5000
                        },
                        initialDelaySeconds: 5
                    }
                }],
                volumes: [{
                    name: 'notion',
                    secret: { secretName: notionSecret.metadata.name }
                }],
                nodeSelector: { 'kubernetes.io/arch': 'arm64' }
            }            
        }
    }
})

new k8s.core.v1.Service(identifier, {
    metadata: {
        name: identifier,
        namespace: config.blogNamespace },
    spec: {
        type: 'ClusterIP',
        selector: labels,
        ports: [{
            name: 'http',
            port: 80,
            targetPort: 5000
        }]
    }
}, { dependsOn: deployment })
