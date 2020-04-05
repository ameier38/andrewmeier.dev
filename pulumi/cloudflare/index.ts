import * as cloudflare from '@pulumi/cloudflare'
import * as config from '../config'
import { inlets } from '../k8s/infrastructure/inlets'

const zone = new cloudflare.Zone(`${config.env}-zone`, {
    zone: config.dnsConfig.tld
}, { provider: config.cloudflareProvider })

export const rootRecord = new cloudflare.Record('root', {
    zoneId: zone.id,
    name: '@',
    type: 'A',
    value: inlets.exitNodeIP
}, { provider: config.cloudflareProvider, deleteBeforeReplace: true })

export const graphqlRecord = new cloudflare.Record('graphql', {
    zoneId: zone.id,
    name: 'graphql',
    type: 'A',
    value: inlets.exitNodeIP
}, { provider: config.cloudflareProvider, deleteBeforeReplace: true })
