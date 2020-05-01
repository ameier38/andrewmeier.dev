import * as pulumi from '@pulumi/pulumi'
import './k8s'
import './dns'
import { exitNode } from './exitNode'
import { cluster } from './k8s/cluster'

export const exitNodeIP = exitNode.exitNodeIP
export const kubeconfig = pulumi.secret(cluster.kubeconfig)
