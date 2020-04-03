import * as pulumi from '@pulumi/pulumi'
import * as k8s from '@pulumi/kubernetes'
import * as config from '../../config'
import { removeTests } from '../transformations'
import { infrastructureNamespaceName } from './namespace'

class Gateway extends pulumi.ComponentResource {
    endpoint: pulumi.Output<string>

    constructor(
            name: string,
            namespace: pulumi.Output<string>,
            opts: pulumi.ComponentResourceOptions) {
        super('infrastructure:Gateway', name, {}, opts)

        const chartVersion = '7.0.0'

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
                ingressRoute: { dashboard: { enabled: false } },
                service: { type: 'ClusterIP' },
                additionalArguments: [
                    // '--log.level=DEBUG',
                ],
            }
        }, { parent: this, dependsOn: [traefikCRDs] })

        const serviceName =
            pulumi.all([namespace, traefikChart])
            .apply(([ns, chart]) => chart.getResourceProperty('v1/Service', ns, 'traefik', 'metadata'))
            .apply(meta => meta.name)

        const servicePort =
            pulumi.all([namespace, traefikChart])
            .apply(([ns, chart]) => chart.getResourceProperty('v1/Service', ns, 'traefik', 'spec'))
            .apply(spec => spec.ports.find(port => port!.name === 'web'))
            .apply(port => port!.port)

        this.endpoint = pulumi.interpolate `${serviceName}.${namespace}.svc.cluster.local:${servicePort}`

        this.registerOutputs({
            endpoint: this.endpoint
        })
    }
}

export const gateway = new Gateway(
    'gateway',
    infrastructureNamespaceName,
    { providers: { k8s: config.k8sProvider, digitalocean: config.digitalOceanProvider } })
