import * as aws from '@pulumi/aws'
import * as awsx from '@pulumi/awsx'
import * as pulumi from '@pulumi/pulumi'
import * as path from 'path'
import * as config from '../config'

const blog = new awsx.ecr.Repository('blog', {
    lifecyclePolicy: {
        rules: [{
            tagStatus: 'any',
            maximumNumberOfImages: 1
        }]
    }
})

const blogImage = new awsx.ecr.Image('blog', {
    repositoryUrl: blog.url,
    path: path.join(config.rootDir, 'app'),
    args: {
        RUNTIME_IMAGE_TAG: '6.0-alpine-arm64v8',
        RUNTIME_ID: 'linux-arm64'
    },
    extraOptions: ['--quiet']
})

export const blogImageUri = blogImage.imageUri

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
