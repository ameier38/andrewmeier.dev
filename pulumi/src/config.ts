import * as pulumi from '@pulumi/pulumi'
import * as docker from '@pulumi/docker'
import * as k8s from '@pulumi/kubernetes'
import * as path from 'path'

export const env = pulumi.getStack()
export const pulumiRoot = path.dirname(__dirname)
export const root = path.dirname(pulumiRoot)

const managedInfrastructureStack = new pulumi.StackReference(`ameier38/managed-infrastructure/${env}`)
const clusterServicesStack = new pulumi.StackReference(`ameier38/cluster-services/${env}`)

export const zone = managedInfrastructureStack.requireOutput('zone').apply(o => o as string)
export const acmeEmail = managedInfrastructureStack.requireOutput('acmeEmail').apply(o => o as string)
export const registryEndpoint = managedInfrastructureStack.requireOutput('registryEndpoint').apply(o => o as string)
export const imageRegistry = managedInfrastructureStack.requireOutput('imageRegistry').apply(o => o as docker.ImageRegistry)
export const dockerCredentials = managedInfrastructureStack.requireOutput('dockerCredentials').apply(o => o as string)
export const kubeconfig = clusterServicesStack.requireOutput('kubeconfig').apply(o => o as string)
export const blogNamespace = clusterServicesStack.requireOutput('blogNamespace').apply(o => o as string)
export const seqHost = clusterServicesStack.requireOutput('seqHost').apply(o => o as string)
export const seqPort = clusterServicesStack.requireOutput('seqPort').apply(o => o as string)

export const k8sProvider = new k8s.Provider('default', {
    kubeconfig: kubeconfig
})

const rawAirtableConfig = new pulumi.Config('airtable')
export const airtableConfig = {
    baseId: rawAirtableConfig.require('baseId'),
    apiKey: rawAirtableConfig.require('apiKey')
}
