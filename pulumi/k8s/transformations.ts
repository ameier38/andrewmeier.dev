export const removeTests = (o:any) => {
    if (o !== undefined) {
        if (o.metadata !== undefined) {
            if (o.metadata.name && o.metadata.name.includes('test')) {
                o.apiVersion = 'v1'
                o.kind = 'List'
                o.items = []
            }
        }
    }
}
