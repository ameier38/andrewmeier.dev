import * as cloudflare from '@pulumi/cloudflare'
import * as config from '../config'
import { gateway } from '../k8s/infrastructure/gateway'

const zone = new cloudflare.Zone(`${config.env}-zone`, {
    zone: config.dnsConfig.tld
}, { provider: config.cloudflareProvider })

export const gatewayWildcardRecord = new cloudflare.Record('gateway-wildcard', {
    zoneId: zone.id,
    name: '*',
    type: 'A',
    value: gateway.gatewayIp
}, { provider: config.cloudflareProvider })

export const gatewayRootRecord = new cloudflare.Record('gateway-root', {
    zoneId: zone.id,
    name: '@',
    type: 'A',
    value: gateway.gatewayIp
}, { provider: config.cloudflareProvider })
