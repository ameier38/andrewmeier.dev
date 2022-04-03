import * as pulumi from '@pulumi/pulumi'
import * as path from 'path'

export const pulumiRoot = path.dirname(__dirname)
export const root = path.dirname(pulumiRoot)

const managedInfrastructureStack = new pulumi.StackReference('ameier38/managed-infrastructure/prod')
const clusterServicesStack = new pulumi.StackReference('ameier38/cluster-services/prod')

export const blogHost = managedInfrastructureStack.requireOutput('blogHost')
export const registryName = managedInfrastructureStack.getOutput('registryName')
export const registryServer = managedInfrastructureStack.requireOutput('registryServer')
export const registryUser = managedInfrastructureStack.getOutput('registryUser')
export const registryPassword = managedInfrastructureStack.getOutput('registryPassword')
export const dockerconfigjson = managedInfrastructureStack.requireOutput('dockerconfigjson')
export const andrewmeierNamespace = clusterServicesStack.requireOutput('andrewmeierNamespace')

const rawNotionConfig = new pulumi.Config('notion')
export const notionConfig = {
    databaseId: rawNotionConfig.requireSecret('databaseId'),
    token: rawNotionConfig.requireSecret('token')
}
