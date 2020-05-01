import * as cloudflare from '@pulumi/cloudflare'
import * as config from '../config'
import { exitNode } from '../exitNode'

const zone = new cloudflare.Zone(`${config.env}-zone`, {
    zone: config.dnsConfig.tld
}, { provider: config.cloudflareProvider })

export const rootRecord = new cloudflare.Record('root', {
    zoneId: zone.id,
    name: '@',
    type: 'A',
    value: exitNode.exitNodeIP
}, { provider: config.cloudflareProvider, deleteBeforeReplace: true })

export const graphqlRecord = new cloudflare.Record('graphql', {
    zoneId: zone.id,
    name: 'graphql',
    type: 'A',
    value: exitNode.exitNodeIP
}, { provider: config.cloudflareProvider, deleteBeforeReplace: true })
