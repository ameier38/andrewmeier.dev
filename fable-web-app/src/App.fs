[<RequireQualifiedAccess>]
module Blog.App

open Elmish
open Feliz
open Feliz.MaterialUI
open Feliz.Router

type Url =
    | HomeUrl
    | AboutUrl
    | PostUrl of Post.Url
    | NotFoundUrl
module Url =
    let parse (url:string list) =
        match url with
        | [] -> HomeUrl
        | [ "about" ] -> AboutUrl
        | [ postId ] -> PostUrl (Post.Url.PostUrl postId)
        | _ -> NotFoundUrl

type State =
    { CurrentUrl: Url
      Home: Home.State
      Post: Post.State }

type Msg =
    | UrlChanged of string list
    | NavigateToHome
    | NavigateToAbout
    | HomeMsg of Home.Msg
    | PostMsg of Post.Msg

let init () : State * Cmd<Msg> =
    let currentUrl = Router.currentPath() |> Url.parse
    match currentUrl with
    | NotFoundUrl
    | AboutUrl
    | HomeUrl ->
        let homeState, homeCmd = Home.init()
        let postState, postCmd = Post.init Post.EmptyUrl
        let cmd =
            [ homeCmd |> Cmd.map HomeMsg
              postCmd |> Cmd.map PostMsg ]
            |> Cmd.batch
        let state =
            { CurrentUrl = currentUrl
              Home = homeState
              Post = postState }
        state, cmd
    | PostUrl postUrl ->
        let homeState, homeCmd = Home.init()
        let postState, postCmd = Post.init postUrl
        let cmd =
            [ homeCmd |> Cmd.map HomeMsg
              postCmd |> Cmd.map PostMsg ]
            |> Cmd.batch
        let state =
            { CurrentUrl = currentUrl
              Home = homeState
              Post = postState }
        state, cmd

let graphqlClient = Graphql.GraphqlClient()

let update (msg:Msg) (state:State): State * Cmd<Msg> =
    Log.info msg
    match msg with
    | UrlChanged url ->
        let currentUrl = Url.parse url
        let newState = { state with CurrentUrl = currentUrl }
        match currentUrl with
        | AboutUrl ->
            newState, Cmd.ofMsg(PostMsg (Post.UrlChanged (Post.PostUrl "about")))
        | PostUrl postUrl ->
            newState, Cmd.ofMsg(PostMsg (Post.UrlChanged postUrl))
        | _ ->
            newState, Cmd.none
    | NavigateToHome ->
        state, Router.navigatePath ""
    | NavigateToAbout ->
        state, Router.navigatePath "about"
    | HomeMsg msg -> 
        let newHome, homeCmd = state.Home |> Home.update graphqlClient msg
        { state with Home = newHome }, homeCmd |> Cmd.map HomeMsg
    | PostMsg msg ->
        let newPost, postCmd = state.Post |> Post.update graphqlClient msg
        { state with Post = newPost }, postCmd |> Cmd.map PostMsg

let useStyles = Styles.makeStyles(fun styles theme ->
    {| 
        appBarOffset = styles.create [
            style.height 80
        ]
        navContainer = styles.create [
            style.display.flex
            style.justifyContent.spaceBetween
        ]
        navHomeButton = styles.create [
            style.fontFamily theme.typography.h6.fontFamily
            style.fontSize 16
            style.fontWeight 500
            style.textTransform.none
        ]
        errorDiv = styles.create [
            style.display.flex
            style.justifyContent.center
        ]
    |}
)

type NavigationProps =
    { isGteMd: bool
      dispatch: Msg -> unit }

let renderNavigation = 
    React.functionComponent<NavigationProps>(fun props ->
        let c = useStyles()
        React.fragment [
            Mui.appBar [
                appBar.variant.outlined
                appBar.color.default'
                appBar.position.fixed'
                appBar.children [
                    Mui.toolbar [
                        Mui.container [
                            prop.className c.navContainer
                            container.disableGutters (not props.isGteMd)
                            container.maxWidth.md
                            container.children [
                                Mui.button [
                                    prop.onClick (fun e ->
                                        e.preventDefault()
                                        props.dispatch NavigateToHome
                                    )
                                    button.classes.root c.navHomeButton
                                    button.children [ "Andrew's Thoughts" ]
                                ]
                                Html.div [
                                    Mui.iconButton [
                                        prop.href "https://twitter.com/ameier38"
                                        iconButton.component' "a"
                                        iconButton.children [
                                            Icons.twitterIcon
                                        ]
                                    ]
                                    Mui.iconButton [
                                        prop.href "https://github.com/ameier38"
                                        iconButton.component' "a"
                                        iconButton.children [
                                            Icons.githubIcon
                                        ]
                                    ]
                                    Mui.button [
                                        prop.onClick (fun e ->
                                            e.preventDefault()
                                            props.dispatch NavigateToAbout
                                        )
                                        button.color.inherit'
                                        button.children [ "About" ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            Html.div [
                prop.className c.appBarOffset
            ]
        ]
    )

let renderPage (state:State) (dispatch:Msg -> unit) =
    match state.CurrentUrl with
    | HomeUrl ->
        Home.render state.Home (HomeMsg >> dispatch)
    | AboutUrl
    | PostUrl _ ->
        Post.render state.Post (PostMsg >> dispatch)
    | NotFoundUrl ->
        Error.renderError "Page does not exist"

type AppProps =
    { state: State
      dispatch: Msg -> unit }

let renderApp =
    React.functionComponent<AppProps>(fun props ->
        let theme = Styles.useTheme()
        let isGteMd = Hooks.useMediaQuery(theme.breakpoints.upMd)
        Mui.container [
            container.disableGutters (not isGteMd)
            container.maxWidth.md
            container.children [
                renderNavigation { dispatch = props.dispatch; isGteMd = isGteMd }
                renderPage props.state props.dispatch
            ]
        ]
    )

let render (state:State) (dispatch:Msg -> unit) =
    Router.router [
        Router.pathMode
        Router.onUrlChanged (UrlChanged >> dispatch)
        Router.application [ 
            Mui.cssBaseline []
            renderApp { state = state; dispatch = dispatch }
        ]
    ]
