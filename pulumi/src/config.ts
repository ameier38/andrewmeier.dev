import * as pulumi from '@pulumi/pulumi'
import * as docker from '@pulumi/docker'
import * as path from 'path'

export const pulumiRoot = path.dirname(__dirname)
export const root = path.dirname(pulumiRoot)

const managedInfrastructureStack = new pulumi.StackReference('ameier38/managed-infrastructure/prod')
const clusterServicesStack = new pulumi.StackReference('ameier38/cluster-services/prod')

export const registryEndpoint = managedInfrastructureStack.requireOutput('registryEndpoint').apply(o => o as string)
export const imageRegistry = managedInfrastructureStack.requireOutput('imageRegistry').apply(o => o as docker.ImageRegistry)
export const dockerCredentials = managedInfrastructureStack.requireOutput('dockerCredentials').apply(o => o as string)
export const blogNamespace = clusterServicesStack.requireOutput('blogNamespace').apply(o => o as string)

const rawNotionConfig = new pulumi.Config('notion')
export const notionConfig = {
    databaseId: rawNotionConfig.requireSecret('databaseId'),
    token: rawNotionConfig.requireSecret('token')
}
