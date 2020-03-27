import * as pulumi from '@pulumi/pulumi'
import * as k8s from '@pulumi/kubernetes'
import * as kx from '@pulumi/kubernetesx'
import * as docker from '@pulumi/docker'
import * as path from 'path'
import * as config from '../../config'
import { appsNamespaceName } from '../infrastructure/namespace'

class Blog extends pulumi.ComponentResource {
    constructor(
            name: string,
            namespace: pulumi.Output<string>,
            opts: pulumi.ComponentResourceOptions) {
        super('apps:Blog', name, {}, opts)

        const blogImage = new docker.Image('blog', {
            imageName: `${config.dockerRegistry.server}/ameier38/blog`,
            build: {
                context: path.join(config.root, 'blog'),
                dockerfile: path.join(config.root, 'blog', 'deploy', 'Dockerfile'),
                extraOptions: [
                    '--platform=linux/arm64'
                ]
            },
            registry: config.dockerRegistry
        }, { parent: this })

        const podBuilder = new kx.PodBuilder({
            containers: [
                {
                    image: blogImage.imageName,
                    imagePullPolicy: 'Always',
                    ports: { http: 8080 },
                }
            ],
        })

        const deployment = new kx.Deployment('blog', {
            metadata: {
                name: 'blog',
                namespace: namespace
            },
            spec: podBuilder.asDeploymentSpec()
        }, { parent: this })

        const service = deployment.createService({
            type: kx.types.ServiceType.ClusterIP
        })

        const ingressRoute = new k8s.apiextensions.CustomResource('blog', {
            apiVersion: 'traefik.containo.us/v1alpha1',
            kind: 'IngressRoute',
            metadata: {
                name: 'blog',
                namespace: namespace
            },
            spec: {
                entryPoints: ['websecure'],
                routes: [
                    {
                        match: 'Host(`andrewmeier.dev`)',
                        kind: 'Rule',
                        services: [ { name: service.metadata.name, port: 8080 } ]
                    }
                ],
                tls: {
                    certResolver: 'default'
                }
            }
        }, { parent: this })
    }
}

export const blog = new Blog(
    'blog',
    appsNamespaceName,
    { provider: config.k8sProvider })
