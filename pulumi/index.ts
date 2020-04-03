import './k8s'
import './cloudflare'
import { gateway } from './k8s/infrastructure/gateway'
import { inlets } from './k8s/infrastructure/inlets'
import { blog } from './k8s/apps/blog'

export const gatewayEndpoint = gateway.endpoint
export const blogEndpoint = blog.endpoint
export const exitNodeIP = inlets.exitNodeIP
