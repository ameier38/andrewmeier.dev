import * as pulumi from '@pulumi/pulumi'
import * as digitalocean from '@pulumi/digitalocean'
import * as config from '../config'

const caddyVersion = '2.0.0-rc.3'
const letsencryptProdEndpoint = 'https://acme-v02.api.letsencrypt.org/directory'
const letsencryptStagingEndpoint = 'https://acme-staging-v02.api.letsencrypt.org/directory'
const letsencryptEndpoint = config.dnsConfig.useStaging ? letsencryptStagingEndpoint : letsencryptProdEndpoint

// ref: https://blog.alexellis.io/https-inlets-local-endpoints/
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

// ref: https://github.com/caddyserver/dist/blob/master/init/caddy.service
const caddyService = String.raw`
[Unit]
Description=Caddy
Documentation=https://caddyserver.com/docs/
After=network.target

[Service]
User=caddy
Group=caddy
ExecStart=/usr/local/bin/caddy run --environ --config /etc/caddy/Caddyfile
ExecReload=/usr/local/bin/caddy reload --config /etc/caddy/Caddyfile
TimeoutStopSec=5s
LimitNOFILE=1048576
LimitNPROC=512
PrivateTmp=true
ProtectSystem=full
AmbientCapabilities=CAP_NET_BIND_SERVICE

[Install]
WantedBy=multi-user.target
`

const inletsEnv = String.raw`
TOKEN=${config.inletsConfig.token}
`

// ref: https://github.com/inlets/inlets/blob/master/hack/inlets.service
const inletsService = String.raw`
[Unit]
Description=Inlets Server Service
After=network.target

[Service]
Type=simple
Restart=always
RestartSec=1
StartLimitInterval=0
EnvironmentFile=/etc/default/inlets
ExecStart=/usr/local/bin/inlets server --port=8080 --disable-transport-wrapping --token="$TOKEN"

[Install]
WantedBy=multi-user.target
`

const userDataScript = String.raw`#!/bin/bash

set -e

# Install caddy
echo "Installing caddy..."
mkdir /tmp/caddy && \
    curl -sL -o /tmp/caddy/caddy.tar.gz \
        https://github.com/caddyserver/caddy/releases/download/v${caddyVersion}/caddy_${caddyVersion}_linux_amd64.tar.gz && \
    tar -C /tmp/caddy -xzf /tmp/caddy/caddy.tar.gz && \
    mv /tmp/caddy/caddy /usr/local/bin/caddy && \
    chmod +x /usr/local/bin/caddy && \
    rm -rf /tmp/caddy

# Set up caddy
echo "Setting up caddy..."
mkdir /etc/caddy
cat << EOF > /etc/caddy/Caddyfile
${caddyfile}
EOF

groupadd --system caddy

useradd --system \
    --gid caddy \
    --create-home \
    --home-dir /var/lib/caddy \
    --shell /usr/sbin/nologin \
    --comment "Caddy web server" \
    caddy

cat << EOF > /etc/systemd/system/caddy.service
${caddyService}
EOF

# Install inlets
echo "Installing inlets..."
curl -sL -o inlets \
    https://github.com/inlets/inlets/releases/download/${config.inletsConfig.version}/inlets && \
    mv inlets /usr/local/bin/inlets && \
    chmod +x /usr/local/bin/inlets

# Set up inlets
echo "Setting up inlets..."
cat << EOF > /etc/default/inlets
${inletsEnv}
EOF

cat << EOF > /etc/systemd/system/inlets.service
${inletsService}
EOF

echo "Starting services..."
systemctl daemon-reload
systemctl enable caddy
systemctl enable inlets
systemctl start caddy
systemctl start inlets
`

class ExitNode extends pulumi.ComponentResource {
    exitNodeIP: pulumi.Output<string>

    constructor(
            name: string,
            opts: pulumi.ComponentResourceOptions) {
        super('infrastructure:ExitNode', name, {}, opts)
        
        const sshKey = new digitalocean.SshKey('default', {
            name: 'default',
            publicKey: config.sshConfig.publicKey
        }, { parent: this })

        const exitNode = new digitalocean.Droplet('exit-node', {
            image: 'ubuntu-18-04-x64',
            region: digitalocean.Regions.NYC1,
            size: digitalocean.DropletSlugs.DropletS1VCPU1GB,
            sshKeys: [sshKey.fingerprint],
            userData: userDataScript
        }, { parent: this })

        this.exitNodeIP = exitNode.ipv4Address

        this.registerOutputs({
            exitNodeIP: this.exitNodeIP
        })
    }
}

export const exitNode = new ExitNode(
    'exit-node',
    { provider: config.digitalOceanProvider })
