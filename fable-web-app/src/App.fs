[<RequireQualifiedAccess>]
module App

open Feliz
open Feliz.MaterialUI
open Feliz.Router

type State =
    { CurrentUrl: string list
      Home: Home.State }

type Msg =
    | UrlChanged of string list
    | HomeMsg of Home.Msg

let init () =
    { CurrentUrl = Router.currentUrl()
      Home = Home.init() }

let update (msg:Msg) (state:State): State =
    printfn "msg: %A" msg
    match msg with
    | UrlChanged url ->
        { state with
            CurrentUrl = url }
    | HomeMsg msg -> 
        { state with
            Home = state.Home |> Home.update msg }

let render (state:State) (dispatch:Msg -> unit) =
    let activePage =
        match state.CurrentUrl with
        | [] -> Home.render state.Home (HomeMsg >> dispatch)
        | [ "about" ] -> 
            Html.div [
                Html.h1 "About"
                Html.h1 (Config.variable "TEST")
            ]
        | [ postId ] -> Html.h1 postId
        | _ -> Html.h1 "Not Found"

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
