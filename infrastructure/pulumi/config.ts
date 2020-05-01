import * as pulumi from '@pulumi/pulumi'
import * as cloudflare from '@pulumi/cloudflare'
import * as digitalocean from '@pulumi/digitalocean'
import * as docker from '@pulumi/docker'
import * as path from 'path'

export const env = pulumi.getStack()
export const root = path.dirname(path.dirname(__dirname))

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
export const k8sConfig = {
    usePi: rawK8sConfig.requireBoolean('useLocal'),
    kubeconfig: rawK8sConfig.require('kubeconfig')
}

const rawDigitalOceanConfig = new pulumi.Config('digitalocean')
export const digitalOceanProvider = new digitalocean.Provider(`${env}-digitalocean-provider`, {
    token: rawDigitalOceanConfig.require('token'),
    spacesEndpoint: 'https://nyc1.digitaloceanspaces.com'
})

const rawCloudflareConfig = new pulumi.Config('cloudflare')
export const cloudflareConfig = {
    email: rawCloudflareConfig.require('email'),
    apiKey: rawCloudflareConfig.require('apiKey')
}
export const cloudflareProvider = new cloudflare.Provider(`${env}-cloudflare-provider`, {
    email: cloudflareConfig.email,
    apiKey: cloudflareConfig.apiKey
})

const rawInletsConfig = new pulumi.Config('inlets')
export const inletsConfig = {
    version: '2.7.0',
    token: rawInletsConfig.require('token')
}

const rawAirtableConfig = new pulumi.Config('airtable')
export const airtableConfig = {
    url: rawAirtableConfig.require('url'),
    apiKey: rawAirtableConfig.require('apiKey')
}
