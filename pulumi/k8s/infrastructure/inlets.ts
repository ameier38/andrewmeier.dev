import * as pulumi from '@pulumi/pulumi'
import * as k8s from '@pulumi/kubernetes'
import * as kx from '@pulumi/kubernetesx'
import * as digitalocean from '@pulumi/digitalocean'
import * as config from '../../config'
import { infrastructureNamespaceName } from './namespace'
import { webApp } from '../apps/webApp'
import { graphql } from '../apps/graphql'

const letsencryptProdEndpoint = 'https://acme-v02.api.letsencrypt.org/directory'
const letsencryptStagingEndpoint = 'https://acme-staging-v02.api.letsencrypt.org/directory'
const letsencryptEndpoint = config.dnsConfig.useStaging ? letsencryptStagingEndpoint : letsencryptProdEndpoint

// ref: https://caddyserver.com/docs/caddyfile/concepts
const caddyfile = String.raw`
{
    email ${config.dnsConfig.email}
    acme_ca ${letsencryptEndpoint}
}

(proxy) {
    reverse_proxy 127.0.0.1:8080
    reverse_proxy /tunnel 127.0.0.1:8080
}

${config.dnsConfig.tld} {
    import proxy
}

graphql.${config.dnsConfig.tld} {
    import proxy
}
`

// ref: https://caddyserver.com/docs/install#manually-installing-as-a-linux-service
const userDataScript = String.raw`#!/bin/bash

set -e

curl -sL https://github.com/caddyserver/caddy/releases/download/v2.0.0-beta.20/caddy2_beta20_linux_amd64 -o caddy && \
    mv caddy /usr/bin/caddy && \
    chmod +x /usr/bin/caddy

groupadd --system caddy

useradd --system \
    --gid caddy \
    --create-home \
    --home-dir /var/lib/caddy \
    --shell /usr/sbin/nologin \
    --comment "Caddy web server" \
    caddy

curl -sLO https://raw.githubusercontent.com/caddyserver/dist/master/init/caddy.service && \
    mv caddy.service /etc/systemd/system/caddy.service

mkdir -p /etc/caddy

cat << EOF > /etc/caddy/Caddyfile
${caddyfile}
EOF

export INLETSTOKEN=${config.inletsConfig.token}

curl -sL https://github.com/inlets/inlets/releases/download/2.7.0/inlets -o inlets && \
    mv inlets /usr/local/bin/inlets && \
    chmod +x /usr/local/bin/inlets

curl -sLO https://raw.githubusercontent.com/inlets/inlets/master/hack/inlets.service  && \
    sed -i s/80/8080/g inlets.service && \
    sed -i 's/^\(ExecStart.*$\)/\1 --disable-transport-wrapping/' inlets.service && \
    mv inlets.service /etc/systemd/system/inlets.service && \
    echo "AUTHTOKEN=$INLETSTOKEN" > /etc/default/inlets

systemctl daemon-reload
systemctl enable caddy
systemctl enable inlets
systemctl start caddy
systemctl start inlets
`

class Inlets extends pulumi.ComponentResource {
    exitNodeIP: pulumi.Output<string>

    constructor(
            name: string,
            namespace: string | pulumi.Output<string>,
            remote: string | pulumi.Output<string>,
            upstream: string | pulumi.Output<string>,
            opts: pulumi.ComponentResourceOptions) {
        super('infrastructure:Inlets', name, {}, opts)
        
        const sshKey = new digitalocean.SshKey('default', {
            name: 'default',
            publicKey: config.sshConfig.publicKey
        }, { parent: this })

        const exitNode = new digitalocean.Droplet('exit-node', {
            image: 'ubuntu-18-04-x64',
            region: digitalocean.Regions.NYC1,
            size: digitalocean.DropletSlugs.Droplet512mb,
            sshKeys: [sshKey.fingerprint],
            userData: userDataScript
        }, { parent: this })

        this.exitNodeIP = exitNode.ipv4Address

        const inletsTokenSecret = new k8s.core.v1.Secret('inlets-token', {
            metadata: {
                namespace: namespace,
                name: 'inlets-token'
            },
            stringData: {
                'token': config.inletsConfig.token
            }
        }, { parent: this })

        const tokenVolumeName = 'inlets-token-volume'
        const tokenMountPath = '/var/inlets'

        const podBuilder = new kx.PodBuilder({
            containers: [
                {
                    name: 'inlets',
                    image: 'inlets/inlets:2.6.3',
                    imagePullPolicy: 'Always',
                    command: ['inlets'],
                    args: [
                        'client',
                        pulumi.interpolate `--remote=wss://${remote}`,
                        pulumi.interpolate `--upstream=${upstream}`,
                        `--token-from=${tokenMountPath}/token`
                    ],
                    volumeMounts: [{ name: tokenVolumeName, mountPath: '/var/inlets' }]
                }
            ],
            volumes: [ { name: tokenVolumeName, secret: { secretName: inletsTokenSecret.metadata.name } } ]
        })

        const deployment = new kx.Deployment('inlets', {
            metadata: {
                name: 'inlets',
                namespace: namespace
            },
            spec: podBuilder.asDeploymentSpec()
        }, { parent: this })

        this.registerOutputs({
            exitNodeIP: this.exitNodeIP,
        })

    }
}

const tld = config.dnsConfig.tld
const upstream = pulumi.interpolate `${tld}=${webApp.endpoint},graphql.${tld}=${graphql.endpoint}`

export const inlets = new Inlets(
    'inlets',
    infrastructureNamespaceName,
    config.dnsConfig.tld,
    upstream,
    { providers: { k8s: config.k8sProvider, digitalocean: config.digitalOceanProvider } })