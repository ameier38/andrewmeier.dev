import React from 'react'
import { useQuery } from 'graphql-hooks'
import {
    Container,
    LinearProgress,
    Typography,
    Grid,
    Card,
    CardContent
} from '@material-ui/core'
import {
    PostSummary,
    Query,
    QueryListPostsArgs
} from '../generated/types'

const LIST_POSTS_QUERY = `
query ListPosts($input:ListPostsInput!) {
    listPosts(input: $input) {
        posts {
            postId
            title
            createdAt
            updatedAt
        }
    }
}
`
interface GalleryItemProps {
    post: PostSummary
}

const GalleryItem : React.FC<GalleryItemProps> = ({post}) => (
    <Grid item>
        <Card>
            <CardContent>
                <Typography variant='h2'>{post.title}</Typography>
                <Typography variant='subtitle1'>Created At: {post.createdAt}</Typography>
                <Typography variant='subtitle1'>Updated At: {post.updatedAt}</Typography>
            </CardContent>
        </Card>
    </Grid>
)

const Gallery : React.FC = () => {
    let { loading, error, data } = useQuery<Query,QueryListPostsArgs>(LIST_POSTS_QUERY, {
        variables: {
            input: {
                pageSize: 10
            }
        }
    })
    if (loading) return <LinearProgress/>
    if (error) return <p>{JSON.stringify(error)}</p>
    return (
        <Grid container spacing={2}>
            {data.listPosts.posts.map(post => (
                <GalleryItem key={post.postId} post={post}/>
            ))}
        </Grid>
    )
} 

const App = () => (
    <Container>
        <Gallery />
    </Container>
)

export default App
