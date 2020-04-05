const getEnv = (key:string) => {
    const value = process.env[key]
    if (typeof value !== 'undefined') {
        return value
    } else {
        throw new Error(`${key} is undefined`)
    }
}

export const appConfig = {
    scheme: getEnv('REACT_APP_SCHEME'),
    host: getEnv('REACT_APP_HOST'),
    port: getEnv('REACT_APP_PORT')
}

export const graphqlConfig = {
    scheme: getEnv('REACT_APP_GRAPHQL_SCHEME'),
    host: getEnv('REACT_APP_GRAPHQL_HOST'),
    port: getEnv('REACT_APP_GRAPHQL_PORT')
}

export const segmentConfig = {
    source: getEnv('REACT_APP_SEGMENT_SOURCE')
}
