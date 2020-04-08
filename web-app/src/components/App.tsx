import React from 'react'
import { Route, Switch, Link, useParams } from 'react-router-dom'
import { useQuery } from 'graphql-hooks'
import { makeStyles } from '@material-ui/core/styles'
import ReactMarkdown from 'react-markdown'
import {
    Container,
    LinearProgress,
    Typography,
    AppBar,
    Toolbar,
    List,
    ListItem,
    ListItemText,
    Card,
    CardMedia,
    CardContent,
    Button
} from '@material-ui/core'
import {
    Query,
    QueryListPostsArgs,
    QueryGetPostArgs
} from '../generated/types'

const ERROR_COW = String.raw`
 -------
< Opps! Something went wrong. >
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
            createdAt
        }
    }
}`

const GET_POST_QUERY = `
query GetPost($input:GetPostInput!) {
    getPost(input: $input) {
        title
        cover
        content
        createdAt
        updatedAt
    }
}`

const useStyles = makeStyles(theme => ({
    appBarOffset: theme.mixins.toolbar,
    toolbar: {
        display: 'flex',
        justifyContent: 'space-between'
    },
    cover: {
        height: 200
    },
    error: {
        display: 'flex',
        justifyContent: 'center'
    }
}))

const TableOfContents : React.FC = () => {
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
        <List>
            {data.listPosts.posts.map(post => (
                <ListItem key={post.postId} component={Link} to={`/${post.postId}`}>
                    <ListItemText primary={post.title} secondary={post.createdAt}/>
                </ListItem>
            ))}
        </List>
    )
} 

const Navigation = () => {
    const classes = useStyles()
    return (
        <React.Fragment>
            <AppBar color='default' position='fixed' elevation={1}>
                <Toolbar>
                    <Container maxWidth='md' className={classes.toolbar}>
                        <Typography variant='h6'>Journal of Andrew Meier</Typography>
                        <div></div>
                        <Button component={Link} to='/about'>About</Button>
                    </Container>
                </Toolbar>
            </AppBar>
            <div className={classes.appBarOffset}></div>
        </React.Fragment>
    )
}

const Detail = () => {
    let classes = useStyles()
    let { postId } = useParams()
    let { loading, error, data } = useQuery<Query,QueryGetPostArgs>(GET_POST_QUERY, {
        variables: {
            input: {
                postId: postId || ''
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
        <Card>
            <CardMedia
                className={classes.cover}
                image={data.getPost.cover}
                title={data.getPost.title}/>
            <CardContent>
                <Typography variant='h2'>{data.getPost.title}</Typography>
                <ReactMarkdown>{data.getPost.content}</ReactMarkdown>
            </CardContent>
        </Card>
    )
}

const App = () => (
    <Container maxWidth='md'>
        <Navigation />
        <Switch>
            <Route exact path="/">
                <TableOfContents/>
            </Route>
            <Route path="/:postId">
                <Detail/>
            </Route>
        </Switch>
    </Container>
)

export default App
