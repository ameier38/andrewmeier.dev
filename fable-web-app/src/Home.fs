[<RequireQualifiedAccess>]
module Blog.Home

open Elmish
open Feliz
open Feliz.MaterialUI
open Feliz.Router
open Graphql

type State =
    { Posts: Deferred<Result<ListPostsResponseDto,string>> }

type Msg =
    | ListPosts of AsyncOperation<unit,Result<ListPostsResponseDto,string>>
    | NavigateToPost of permalink:string 

let listPosts (graphql:IGraphqlClient): Cmd<Msg> =
    async {
        let! response = graphql.ListPosts()
        return ListPosts (Finished response)
    } |> Cmd.fromAsync

let init (): State * Cmd<Msg> =
    let state = { Posts = NotStarted }
    state, Cmd.ofMsg(ListPosts (Started ()))

let update (graphql:IGraphqlClient) (msg:Msg) (state:State): State * Cmd<Msg> =
    match msg with
    | NavigateToPost permalink -> 
        state, Router.navigatePath permalink
    | ListPosts (Started ()) ->
        { state with Posts = InProgress }, listPosts graphql
    | ListPosts (Finished response) ->
        { state with Posts = Resolved response }, Cmd.none

let renderPosts (posts:PostSummaryDto list) (dispatch:Msg -> unit) =
    Mui.list [
        for post in posts do
            yield Mui.listItem [
                prop.key post.permalink
                prop.onClick (fun e -> 
                    e.preventDefault()
                    dispatch (NavigateToPost post.permalink)
                )
                listItem.button true
                listItem.children [
                    Mui.listItemText [
                        listItemText.primary (Mui.typography [
                            typography.gutterBottom true
                            typography.variant.h6
                            typography.children post.title
                        ])
                        listItemText.secondary (post.createdAt.ToString())
                    ]
                ]
            ]
            yield Mui.divider []
    ]

let renderSkeleton () =
    let listItems =
        [ for i in 1..10 do
            Mui.listItem [
                prop.key i
                listItem.children [
                    Mui.skeleton [
                        skeleton.variant.text
                        skeleton.animation.pulse
                    ]
                    Mui.skeleton [
                        skeleton.variant.text
                        skeleton.animation.pulse
                    ]
                ]
            ]
        ]
    Mui.list listItems

let render (state:State) (dispatch:Msg -> unit) =
    match state.Posts with
    | NotStarted
    | InProgress ->
        renderSkeleton()
    | Resolved response ->
        match response with
        | Ok listPostsResponse ->
            renderPosts listPostsResponse.posts dispatch
        | Error error ->
            Error.renderError error
