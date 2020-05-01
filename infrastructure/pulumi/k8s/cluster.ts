import * as pulumi from '@pulumi/pulumi'
import * as k8s from '@pulumi/kubernetes'
import * as digitalocean from '@pulumi/digitalocean'
import * as config from '../config'

// ref: https://www.digitalocean.com/community/tutorials/how-to-manage-digitalocean-and-kubernetes-infrastructure-with-pulumi
class Cluster extends pulumi.ComponentResource {
    kubeconfig: string | pulumi.Output<string>

    constructor(name:string, opts:pulumi.ComponentResourceOptions) {
        super('infrastructure:Cluster', name, {}, opts)

        if (config.k8sConfig.usePi) {
            this.kubeconfig = config.k8sConfig.kubeconfig
        } else {
            const cluster = new digitalocean.KubernetesCluster(`${config.env}-cluster`, {
                region: digitalocean.Regions.NYC1,
                version: '1.16.6-do.2',
                nodePool: {
                    name: 'default',
                    size: digitalocean.DropletSlugs.DropletS1VCPU2GB,
                    nodeCount: 3
                }
            }, { parent: this })

            this.kubeconfig = cluster.kubeConfigs[0].rawConfig
        }

        this.registerOutputs({
            kubeconfig: this.kubeconfig
        })
    }
}

export const cluster = new Cluster(
    'default',
    {provider: config.digitalOceanProvider})

export const k8sProvider = new k8s.Provider(`${config.env}-k8s-provider`, {
    kubeconfig: cluster.kubeconfig
})
