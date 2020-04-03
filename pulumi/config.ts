import * as pulumi from '@pulumi/pulumi'
import * as cloudflare from '@pulumi/cloudflare'
import * as digitalocean from '@pulumi/digitalocean'
import * as k8s from '@pulumi/kubernetes'
import * as docker from '@pulumi/docker'
import * as path from 'path'

export const env = pulumi.getStack()
export const root = path.dirname(__dirname)

const rawDnsConfig = new pulumi.Config('dns')
export const dnsConfig = {
    tld: rawDnsConfig.require('tld'),
    email: rawDnsConfig.require('email'),
    useStaging: rawDnsConfig.requireBoolean('useStaging')
}

const rawSshConfig = new pulumi.Config('ssh')
export const sshConfig = {
    publicKey: rawSshConfig.require('publicKey')
}

const rawDockerConfig = new pulumi.Config('docker')
export const dockerRegistry: docker.ImageRegistry = {
    server: 'docker.io',
    username: rawDockerConfig.require('user'),
    password: rawDockerConfig.require('password')
}

const rawK8sConfig = new pulumi.Config('k8s')
export const k8sProvider = new k8s.Provider(`${env}-k8s-provider`, {
    kubeconfig: rawK8sConfig.require('kubeconfig')
})

const rawDigitalOceanConfig = new pulumi.Config('digitalocean')
export const digitalOceanProvider = new digitalocean.Provider(`${env}-digitalocean-provider`, {
    token: rawDigitalOceanConfig.require('token')
})

const rawCloudflareConfig = new pulumi.Config('cloudflare')
export const cloudflareProvider = new cloudflare.Provider(`${env}-cloudflare-provider`, {
    email: rawCloudflareConfig.require('email'),
    apiKey: rawCloudflareConfig.require('apiKey')
})

const rawInletsConfig = new pulumi.Config('inlets')
export const inletsConfig = {
    token: rawInletsConfig.require('token')
}
