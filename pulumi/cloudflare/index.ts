import * as cloudflare from '@pulumi/cloudflare'
import * as config from '../config'
import { inlets } from '../k8s/infrastructure/inlets'

const zone = new cloudflare.Zone(`${config.env}-zone`, {
    zone: config.dnsConfig.tld
}, { provider: config.cloudflareProvider })

export const inletsRootRecord = new cloudflare.Record('inlets-root', {
    zoneId: zone.id,
    name: '@',
    type: 'A',
    value: inlets.exitNodeIP
}, { provider: config.cloudflareProvider })
