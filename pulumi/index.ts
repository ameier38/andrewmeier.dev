import './k8s'
import './cloudflare'
import { gateway } from './k8s/infrastructure/gateway'
import { inlets } from './k8s/infrastructure/inlets'
import { webApp } from './k8s/apps/webApp'

export const gatewayEndpoint = gateway.endpoint
export const webAppEndpoint = webApp.endpoint
export const exitNodeIP = inlets.exitNodeIP
