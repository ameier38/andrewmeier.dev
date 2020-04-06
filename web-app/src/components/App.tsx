import React from 'react'
import { useQuery } from 'graphql-hooks'
import { makeStyles } from '@material-ui/core/styles'
import ReactMarkdown from 'react-markdown'
import {
    Container,
    LinearProgress,
    Typography,
    Grid,
    Card,
    CardContent
} from '@material-ui/core'
import {
    Post,
    Query,
    QueryListPostsArgs
} from '../generated/types'

const ERROR_COW = String.raw`
 -------
< Opps! >
 -------
        \   ^__^
         \  (xx)\_______
            (__)\       )\/\
             U  ||----w |
                ||     ||
`

const LIST_POSTS_QUERY = `
query ListPosts($input:ListPostsInput!) {
    listPosts(input: $input) {
        posts {
            postId
            title
            content
            createdAt
            updatedAt
        }
    }
}`

const useStyles = makeStyles({
    jumbotron: {
        width: '100%',
        height: 100,
        textAlign: 'center'
    },
    error: {
        display: 'flex',
        justifyContent: 'center'
    }
})

const Jumbotron = () => {
    const classes = useStyles()
    return (
        <div className={classes.jumbotron}>
            <Typography variant='h2'>Andrew's Blog</Typography>
        </div>
    )
}

interface GalleryItemProps {
    post: Post
}

const GalleryItem : React.FC<GalleryItemProps> = ({post}) => (
    <Grid item xs={12}>
        <Card>
            <CardContent>
                <Typography variant='h2'>{post.title}</Typography>
                <Typography variant='subtitle1'>Created At: {post.createdAt}</Typography>
                <Typography variant='subtitle1'>Updated At: {post.updatedAt}</Typography>
                <ReactMarkdown>{post.content}</ReactMarkdown>
            </CardContent>
        </Card>
    </Grid>
)

const Gallery : React.FC = () => {
    let classes = useStyles()
    let { loading, error, data } = useQuery<Query,QueryListPostsArgs>(LIST_POSTS_QUERY, {
        variables: {
            input: {
                pageSize: 10
            }
        }
    })
    if (loading) return <LinearProgress/>
    if (error) return (
        <div className={classes.error}>
            <pre>{ERROR_COW}</pre>
        </div>
    )
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
        <Jumbotron />
        <Gallery />
    </Container>
)

export default App
