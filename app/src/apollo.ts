import ApolloClient from 'apollo-boost'
import { graphqlConfig } from './config'

const { scheme, host, port } = graphqlConfig

const uri = 
    ['', '80'].includes(port)
    ? `${scheme}://${host}` 
    : `${scheme}://${host}:${port}`

export const client = new ApolloClient({
    uri: uri
})
