import * as cloudflare from '@pulumi/cloudflare'
import * as config from '../config'
import { exitNode } from '../digitalocean/exitNode'

const zone = new cloudflare.Zone(`${config.env}-zone`, {
    zone: config.dnsConfig.tld
}, { provider: config.cloudflareProvider })

export const rootRecord = new cloudflare.Record('root', {
    zoneId: zone.id,
    name: '@',
    type: 'A',
    value: exitNode.exitNodeIP
}, { provider: config.cloudflareProvider, deleteBeforeReplace: true })

export const wildcardRecord = new cloudflare.Record('wildcard', {
    zoneId: zone.id,
    name: '*',
    type: 'A',
    value: exitNode.exitNodeIP
}, { provider: config.cloudflareProvider, deleteBeforeReplace: true })
