import * as k8s from '@pulumi/kubernetes'
import { k8sProvider } from '../cluster'

const infrastructureNamespace = new k8s.core.v1.Namespace('infrastructure', {
    metadata: {
        name: 'infrastructure'
    }
}, { provider: k8sProvider })

export const infrastructureNamespaceName = infrastructureNamespace.metadata.name

const appsNamespace = new k8s.core.v1.Namespace('apps', {
    metadata: {
        name: 'apps'
    }
}, { provider: k8sProvider })

export const appsNamespaceName = appsNamespace.metadata.name
