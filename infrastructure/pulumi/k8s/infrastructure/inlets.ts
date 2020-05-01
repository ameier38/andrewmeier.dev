import * as pulumi from '@pulumi/pulumi'
import * as k8s from '@pulumi/kubernetes'
import * as config from '../../config'
import { k8sProvider } from '../cluster'
import { infrastructureNamespaceName } from './namespace'
import { webApp } from '../apps/webApp'
import { graphqlApi } from '../apps/graphqlApi'

class Inlets extends pulumi.ComponentResource {
    deploymentName: pulumi.Output<string>

    constructor(
            name: string,
            namespace: string | pulumi.Output<string>,
            remote: string | pulumi.Output<string>,
            upstream: string | pulumi.Output<string>,
            opts: pulumi.ComponentResourceOptions) {
        super('infrastructure:Inlets', name, {}, opts)
        
        const inletsTokenSecret = new k8s.core.v1.Secret('inlets-token', {
            metadata: {
                namespace: namespace,
                name: 'inlets-token'
            },
            stringData: {
                'token': config.inletsConfig.token
            }
        }, { parent: this })

        const labels = { 'app.kubernetes.io/name': 'inlets' }
        const tokenVolumeName = 'inlets-token-volume'
        const tokenMountPath = '/var/inlets'

        const deployment = new k8s.apps.v1.Deployment('inlets', {
            metadata: { name: 'inlets', namespace: infrastructureNamespaceName },
            spec: {
                replicas: 1,
                selector: { 
                    matchLabels: labels
                },
                template: {
                    metadata: {
                        labels: labels
                    },
                    spec: {
                        containers: [{
                            name: 'inlets',
                            image: `inlets/inlets:${config.inletsConfig.version}`,
                            imagePullPolicy: 'Always',
                            command: ['inlets'],
                            args: [
                                'client',
                                pulumi.interpolate `--remote=wss://${remote}`,
                                pulumi.interpolate `--upstream=${upstream}`,
                                `--token-from=${tokenMountPath}/token`
                            ],
                            volumeMounts: [{ name: tokenVolumeName, mountPath: tokenMountPath }]
                        }],
                        volumes: [{
                            name: tokenVolumeName,
                            secret: { secretName: inletsTokenSecret.metadata.name } 
                        }]
                    }
                }
            }
        }, { parent: this })

        this.deploymentName = deployment.metadata.name

        this.registerOutputs({
            deploymentName: this.deploymentName,
        })
    }
}

const tld = config.dnsConfig.tld
const upstream = pulumi.interpolate `${tld}=${webApp.endpoint},graphql.${tld}=${graphqlApi.endpoint}`

export const inlets = new Inlets(
    'inlets',
    infrastructureNamespaceName,
    config.dnsConfig.tld,
    upstream,
    { provider: k8sProvider })
