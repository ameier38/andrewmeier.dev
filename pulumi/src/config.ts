import * as pulumi from '@pulumi/pulumi'
import * as path from 'path'

export const pulumiRootDir = path.dirname(__dirname)
export const rootDir = path.dirname(pulumiRootDir)

const clusterServicesStack = new pulumi.StackReference('ameier38/cluster-services/prod')
const appServicesStack = new pulumi.StackReference('ameier38/app-services/prod')

export const andrewmeierNamespace = clusterServicesStack.requireOutput('andrewmeierNamespace')
export const blogHost = appServicesStack.requireOutput('blogHost')

const rawNotionConfig = new pulumi.Config('notion')
export const notionConfig = {
    databaseId: rawNotionConfig.requireSecret('databaseId'),
    token: rawNotionConfig.requireSecret('token')
}
