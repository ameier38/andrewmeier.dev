[<RequireQualifiedAccess>]
module Client.Page.Home

open Client
open Elmish
open Feliz
open Feliz.MaterialUI
open Feliz.Router
open Shared.Api
open Shared.Domain

type State =
    { Posts: Deferred<PostSummary list> }

type Msg =
    | ListPosts
    | NavigateToPost of permalink:string 
    | PostsReceived of PostSummary list
    | ErrorReceived of exn

let listPosts() =
    async {
        let req = { PageToken = None; PageSize = Some 100 }
        let! res = Client.Api.postApi.listPosts req
        return res.Posts
    }

let init (): State * Cmd<Msg> =
    { Posts = HasNotStarted }, Cmd.OfAsync.either listPosts () PostsReceived ErrorReceived

let update (msg:Msg) (state:State): State * Cmd<Msg> =
    match msg with
    | NavigateToPost permalink -> 
        state, Cmd.navigatePath permalink
    | ListPosts ->
        { state with Posts = InProgress }, Cmd.OfAsync.either listPosts () PostsReceived ErrorReceived
    | PostsReceived posts ->
        { state with Posts = Resolved posts }, Cmd.none
    | ErrorReceived err ->
        Browser.Dom.console.error(err)
        { state with Posts = Error err.Message }, Cmd.none

let renderPosts (posts:PostSummary list) (dispatch:Msg -> unit) =
    Mui.list [
        for post in posts do
            Mui.listItem [
                prop.className "post"
                prop.id post.Permalink
                prop.key post.Permalink
                prop.onClick (fun e -> 
                    e.preventDefault()
                    dispatch (NavigateToPost post.Permalink)
                )
                listItem.button true
                listItem.children [
                    Mui.listItemText [
                        listItemText.primary (Mui.typography [
                            typography.gutterBottom true
                            typography.variant.h6
                            typography.children post.Title
                        ])
                        listItemText.secondary (post.CreatedAt.ToString())
                    ]
                ]
            ]
            Mui.divider []
    ]

let renderSkeleton () =
    Mui.list [
        for i in 1..5 do
            Mui.listItem [
                prop.key i
                listItem.children [
                    Mui.listItemText [
                        listItemText.primary (
                            Mui.skeleton [
                                skeleton.animation.wave
                                skeleton.width 100
                            ]
                        )
                        listItemText.secondary (
                            Mui.skeleton [
                                skeleton.animation.wave
                                skeleton.width 250
                            ]
                        )
                    ]
                ]
            ]
            Mui.divider []
    ]

let renderError (msg:string) =
    Mui.dialog [
        Mui.dialogTitle "Error"
        Mui.dialogContent [
            Mui.dialogContentText msg
        ]
    ]

let render (state:State) (dispatch:Msg -> unit) =
    match state.Posts with
    | HasNotStarted
    | InProgress ->
        renderSkeleton()
    | Error msg ->
        renderError msg
    | Resolved posts ->
        renderPosts posts dispatch
