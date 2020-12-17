import * as k8s from '@pulumi/kubernetes'
import * as config from './config'

export const blogNamespace = new k8s.core.v1.Namespace('blog', {
    metadata: { name: 'blog' }
}, { provider: config.k8sProvider })

export const localBlogNamespace = new k8s.core.v1.Namespace('local-blog', {
    metadata: { name: 'blog' }
}, { provider: config.localK8sProvider })
