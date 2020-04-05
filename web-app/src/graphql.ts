import { GraphQLClient } from 'graphql-hooks'
import { graphqlConfig } from './config'

const { scheme, host, port } = graphqlConfig

const url = 
    ['', '80'].includes(port)
    ? `${scheme}://${host}` 
    : `${scheme}://${host}:${port}`

export const client = new GraphQLClient({
    url: url
})
