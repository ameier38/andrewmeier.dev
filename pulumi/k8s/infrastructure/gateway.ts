import * as pulumi from '@pulumi/pulumi'
import * as k8s from '@pulumi/kubernetes'
import * as kq from '@pulumi/query-kubernetes'
import * as config from '../../config'
import { removeTests } from '../transformations'
import { infrastructureNamespaceName } from './namespace'
import { inlets } from './inlets'

class Gateway extends pulumi.ComponentResource {
    gatewayIp: string | pulumi.Output<string>

    constructor(
            name: string,
            namespace: pulumi.Output<string>,
            opts: pulumi.ComponentResourceOptions) {
        super('infrastructure:Gateway', name, {}, opts)

        const chartVersion = '7.0.0'

        const cloudflareSecret = new k8s.core.v1.Secret('cloudflare', {
            metadata: {
                namespace: namespace
            },
            stringData: {
                'email': config.cloudflareConfig.email,
                'api-key': config.cloudflareConfig.apiToken
            }
        }, { parent: this })

        const traefikCRDs = new k8s.yaml.ConfigGroup('traefik-crds', {
            files: [
                'ingressroute.yaml',
                'ingressroutetcp.yaml',
                'ingressrouteudp.yaml',
                'middlewares.yaml',
                'tlsoptions.yaml',
                'tlsstores.yaml',
                'traefikservices.yaml'
            ].map(file => `https://raw.githubusercontent.com/containous/traefik-helm-chart/master/traefik/crds/${file}`)
        }, { parent: this })

        const traefikChart = new k8s.helm.v3.Chart('traefik', {
            chart: 'traefik',
            version: chartVersion,
            namespace: namespace,
            transformations: [removeTests],
            fetchOpts: {
                repo: 'https://containous.github.io/traefik-helm-chart'
            },
            values: {
                additionalArguments: [
                    '--log.level=DEBUG',
                    '--entrypoints.web.http.redirections.entryPoint.to=websecure',
                    '--entrypoints.web.http.redirections.entryPoint.scheme=https',
                    '--certificatesResolvers.default.acme.dnsChallenge.provider=cloudflare',
                    '--certificatesresolvers.default.acme.caserver=https://acme-staging-v02.api.letsencrypt.org/directory'
                ],
                service: {
                    annotations: {
                        // FIXME: https://github.com/inlets/inlets-operator/issues/70
                        'pulumi.com/skipAwait': "true"
                    }
                },
                env: [
                    { 
                        name: 'CF_API_EMAIL',
                        valueFrom: {
                            secretKeyRef: {
                                name: cloudflareSecret.metadata.name,
                                key: 'email'
                            }
                        }
                    },
                    { 
                        name: 'CF_API_KEY',
                        valueFrom: {
                            secretKeyRef: {
                                name: cloudflareSecret.metadata.name,
                                key: 'api-key'
                            }
                        }
                    }
                ]
            }
        }, { parent: this, dependsOn: [traefikCRDs] })

        // FIXME: https://github.com/inlets/inlets-operator/issues/70
        this.gatewayIp = '161.35.8.149'

        this.registerOutputs({
            gatewayIp: this.gatewayIp,
        })
    }
}

export const gateway = new Gateway(
    'gateway',
    infrastructureNamespaceName,
    { provider: config.k8sProvider, dependsOn: [inlets] })
