import * as pulumi from '@pulumi/pulumi'
import * as k8s from '@pulumi/kubernetes'
import * as kx from '@pulumi/kubernetesx'
import * as config from '../../config'
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

        const tokenVolumeName = 'inlets-token-volume'
        const tokenMountPath = '/var/inlets'

        const podBuilder = new kx.PodBuilder({
            containers: [
                {
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
                    volumeMounts: [{ name: tokenVolumeName, mountPath: '/var/inlets' }]
                }
            ],
            volumes: [ { name: tokenVolumeName, secret: { secretName: inletsTokenSecret.metadata.name } } ]
        })

        const deployment = new kx.Deployment('inlets', {
            metadata: {
                name: 'inlets',
                namespace: namespace
            },
            spec: podBuilder.asDeploymentSpec()
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
    { providers: { k8s: config.k8sProvider, digitalocean: config.digitalOceanProvider } })
