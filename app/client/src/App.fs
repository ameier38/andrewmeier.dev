[<RequireQualifiedAccess>]
module Client.App

open Elmish
open Feliz
open Feliz.MaterialUI
open Feliz.Router

type Url =
    | HomeUrl
    | PostUrl of Page.Post.Url
module Url =
    let parse (url:string list) =
        match url with
        | [] -> HomeUrl
        | permalink :: _ -> PostUrl (Page.Post.Url.PostUrl permalink)

type State =
    { CurrentUrl: Url
      Home: Page.Home.State
      Post: Page.Post.State }

type Msg =
    | NavigateToHome
    | NavigateToAbout
    | UrlChanged of string list
    | HomeMsg of Page.Home.Msg
    | PostMsg of Page.Post.Msg

let init () : State * Cmd<Msg> =
    let currentUrl = Router.currentPath() |> Url.parse
    match currentUrl with
    | HomeUrl ->
        let homeState, homeCmd = Page.Home.init()
        let postState, postCmd = Page.Post.init Page.Post.EmptyUrl
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
        let homeState, homeCmd = Page.Home.init()
        let postState, postCmd = Page.Post.init postUrl
        let cmd =
            [ homeCmd |> Cmd.map HomeMsg
              postCmd |> Cmd.map PostMsg ]
            |> Cmd.batch
        let state =
            { CurrentUrl = currentUrl
              Home = homeState
              Post = postState }
        state, cmd

let update (msg:Msg) (state:State): State * Cmd<Msg> =
    match msg with
    | NavigateToHome ->
        state, Cmd.navigatePath ""
    | NavigateToAbout ->
        state, Cmd.navigatePath "about"
    | UrlChanged url ->
        let currentUrl = Url.parse url
        let newState = { state with CurrentUrl = currentUrl }
        match currentUrl with
        | PostUrl postUrl ->
            newState, Cmd.ofMsg(PostMsg (Page.Post.UrlChanged postUrl))
        | _ ->
            newState, Cmd.none
    | HomeMsg msg -> 
        let newHome, homeCmd = state.Home |> Page.Home.update msg
        { state with Home = newHome }, homeCmd |> Cmd.map HomeMsg
    | PostMsg msg ->
        let newPost, postCmd = state.Post |> Page.Post.update msg
        { state with Post = newPost }, postCmd |> Cmd.map PostMsg

let useStyles = Styles.makeStyles(fun styles _ ->
    {| 
        appBarOffset = styles.create [
            style.height 80
        ]
        navContainer = styles.create [
            style.display.flex
            style.justifyContent.spaceBetween
        ]
        navHomeButton = styles.create [
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
        Page.Home.render state.Home (HomeMsg >> dispatch)
    | PostUrl _ ->
        Page.Post.render state.Post (PostMsg >> dispatch)

type AppProps =
    { state: State
      dispatch: Msg -> unit }

let renderApp =
    React.functionComponent<AppProps>(fun props ->
        let isGteMd = Hooks.useMediaQuery("max-width: 960px")
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
    React.router [
        router.pathMode
        router.onUrlChanged (UrlChanged >> dispatch)
        router.children [ 
            Mui.cssBaseline []
            renderApp { state = state; dispatch = dispatch }
        ]
    ]
