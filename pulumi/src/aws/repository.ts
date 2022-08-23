import * as aws from '@pulumi/aws'
import * as awsx from '@pulumi/awsx'
import * as pulumi from '@pulumi/pulumi'
import * as path from 'path'
import * as config from '../config'

const repoLifeCyclePolicyArgs : awsx.ecr.LifecyclePolicyArgs = {
    rules: [
        {
            selection: 'any',
            maximumNumberOfImages: 1
        }
    ]
}

const blog = new awsx.ecr.Repository('blog', {
    lifeCyclePolicyArgs: repoLifeCyclePolicyArgs,
})

const blogCredentials = aws.ecr.getCredentialsOutput({ registryId: blog.repository.registryId })

export const blogDockerconfigjson =
    pulumi
        .all([blogCredentials, blog.repository.repositoryUrl])
        .apply(([creds, repoUrl]) => {
            return JSON.stringify({
                auths: {
                    [repoUrl]: {
                        auth: creds.authorizationToken
                    }
                }
            })
        })

export const blogImageName = blog.buildAndPushImage({ 
    context: path.join(config.rootDir, 'app'),
    args: {
        RUNTIME_IMAGE_TAG: '6.0-alpine-arm64v8',
        RUNTIME_ID: 'linux-arm64'
    },
    extraOptions: ['--quiet']
})
