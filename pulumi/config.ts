import * as pulumi from '@pulumi/pulumi'
import * as cloudflare from '@pulumi/cloudflare'
import * as k8s from '@pulumi/kubernetes'
import * as docker from '@pulumi/docker'
import * as path from 'path'

export const env = pulumi.getStack()
export const root = path.dirname(__dirname)

const rawDnsConfig = new pulumi.Config('dns')
export const dnsConfig = {
    tld: rawDnsConfig.require('tld')
}

const rawDockerConfig = new pulumi.Config('docker')
export const dockerRegistry: docker.ImageRegistry = {
    server: 'docker.io',
    username: rawDockerConfig.require('user'),
    password: rawDockerConfig.require('password')
}

const rawK8sConfig = new pulumi.Config('k8s')
const k8sConfig = {
    kubeconfig: rawK8sConfig.require('kubeconfig')
}
export const k8sProvider = new k8s.Provider(`${env}-k8s-provider`, {
    kubeconfig: k8sConfig.kubeconfig
})

const rawDigitalOceanConfig = new pulumi.Config('digitalOcean')
export const digitalOceanConfig = {
    accessToken: rawDigitalOceanConfig.require('accessToken')
}

const rawCloudflareConfig = new pulumi.Config('cloudflare')
export const cloudflareConfig = {
    email: rawCloudflareConfig.require('email'),
    apiToken: rawCloudflareConfig.require('apiToken')
}
export const cloudflareProvider = new cloudflare.Provider(`${env}-cloudflare-provider`, {
    email: cloudflareConfig.email,
    apiKey: cloudflareConfig.apiToken
})

