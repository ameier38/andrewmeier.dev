[<RequireQualifiedAccess>]
module Blog.App

open Feliz
open Feliz.MaterialUI
open Feliz.Router

let cowNotFound = @"
 --------------------------
< That page does not exist >
 --------------------------
        \   ^__^
         \  (xx)\_______
            (__)\       )\/\
             U  ||----w |
                ||     ||
"

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
    | HomeMsg of Home.Msg
    | PostMsg of Post.Msg

let init () =
    let currentUrl = Router.currentUrl() |> Url.parse
    match currentUrl with
    | NotFoundUrl
    | AboutUrl
    | HomeUrl ->
        { CurrentUrl = currentUrl
          Home = Home.init()
          Post = Post.init Post.EmptyUrl }
    | PostUrl postUrl ->
        { CurrentUrl = currentUrl
          Home = Home.init()
          Post = Post.init postUrl }

let update (msg:Msg) (state:State): State =
    match msg with
    | UrlChanged url ->
        let currentUrl = url |> Url.parse
        match currentUrl with
        | PostUrl postUrl ->
            let postMsg = Post.UrlChanged postUrl
            { state with
                CurrentUrl = currentUrl
                Post = state.Post |> Post.update postMsg }
        | _ ->
            { state with
                CurrentUrl = currentUrl }
    | HomeMsg msg -> 
        { state with
            Home = state.Home |> Home.update msg }
    | PostMsg msg ->
        { state with
            Post = state.Post |> Post.update msg }

// let useStyle = Styles.makeStyles(fun styles theme ->
//     {| 
//         notFound = styles.create [
//             style.display.flex
//             style.justifyContent.center
//         ]
//     |}
// )

let render (state:State) (dispatch:Msg -> unit) =
    printfn "state: %A" state
    let activePage =
        match state.CurrentUrl with
        | HomeUrl -> Home.render state.Home (HomeMsg >> dispatch)
        | AboutUrl -> 
            Html.div [
                Html.h1 "About"
                Html.h1 (Env.getEnv "TEST")
            ]
        | PostUrl _ -> Post.render state.Post (PostMsg >> dispatch)
        | NotFoundUrl ->
            Html.div [
                prop.children [
                    Mui.typography [
                        typography.variant.h1
                    ]
                    Html.pre cowNotFound
                ]
            ]

    Router.router [
        Router.onUrlChanged (UrlChanged >> dispatch)
        Router.application [
            Mui.container [
                container.maxWidth.md
                container.children [
                    activePage
                ]
            ]
        ]
    ]
