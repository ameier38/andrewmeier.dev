import * as pulumi from '@pulumi/pulumi'
import * as k8s from '@pulumi/kubernetes'
import * as config from '../../config'
import { infrastructureNamespaceName } from './namespace'

// ref: https://github.com/inlets/inlets-operator/tree/master/chart/inlets-operator
class Inlets extends pulumi.ComponentResource {
    constructor(
            name: string,
            namespace: pulumi.Output<string>,
            opts: pulumi.ComponentResourceOptions) {
        super('infrastructure:Inlets', name, {}, opts)

        const chartVersion = '0.7.0'

        const inletsAccessKeySecret = new k8s.core.v1.Secret('inlets-access-key', {
            metadata: {
                namespace: namespace,
                name: 'inlets-access-key'
            },
            stringData: {
                'inlets-access-key': config.digitalOceanConfig.accessToken
            }
        }, { parent: this })

        const inletsCRD = new k8s.yaml.ConfigFile('inlets-crd', {
            file: `https://raw.githubusercontent.com/inlets/inlets-operator/${chartVersion}/artifacts/crd.yaml`
        }, { parent: this })

        const inletsOperatorChart = new k8s.helm.v3.Chart('inlets-operator', {
            chart: 'inlets-operator',
            version: chartVersion,
            fetchOpts: {
                repo: 'https://inlets.github.io/inlets-operator'
            },
            namespace: namespace,
            values: {
                provider: 'digitalocean',
                region: 'nyc1',
            }
        }, { parent: this, dependsOn: [inletsAccessKeySecret, inletsCRD] })
    }
}

export const inlets = new Inlets(
    'inlets',
    infrastructureNamespaceName,
    { provider: config.k8sProvider })
