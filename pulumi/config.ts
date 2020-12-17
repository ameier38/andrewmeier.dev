import * as pulumi from '@pulumi/pulumi'
import * as cloudflare from '@pulumi/cloudflare'
import * as docker from '@pulumi/docker'
import * as k8s from '@pulumi/kubernetes'
import * as path from 'path'

export const env = pulumi.getStack()
export const root = path.dirname(__dirname)

const infrastructureStack = new pulumi.StackReference('ameier38/infrastructure/prod')

export const zone = infrastructureStack.requireOutput('zone').apply(o => o as string)
export const zoneId = infrastructureStack.requireOutput('zoneId').apply(o => o as string)
export const acmeEmail = infrastructureStack.requireOutput('acmeEmail').apply(o => o as string)
export const registryEndpoint = infrastructureStack.requireOutput('registryEndpoint').apply(o => o as string)
export const imageRegistry = infrastructureStack.requireOutput('imageRegistry').apply(o => o as docker.ImageRegistry)
export const dockerCredentials = infrastructureStack.requireOutput('dockerCredentials').apply(o => o as string)
export const kubeconfig = infrastructureStack.requireOutput('localKubeconfig').apply(o => o as string)

export const k8sProvider = new k8s.Provider(`${env}-k8s-provider`, {
    kubeconfig: kubeconfig
})

const rawCloudflareConfig = new pulumi.Config('cloudflare')
export const cloudflareProvider = new cloudflare.Provider(`${env}-cloudflare-provider`, {
    email: rawCloudflareConfig.require('email'),
    apiKey: rawCloudflareConfig.require('apiKey')
})

const rawAirtableConfig = new pulumi.Config('airtable')
export const airtableConfig = {
    baseId: rawAirtableConfig.require('baseId'),
    apiKey: rawAirtableConfig.require('apiKey')
}
