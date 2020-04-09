[<RequireQualifiedAccess>]
module Home

open Feliz
open Feliz.MaterialUI
open System

type PostSummary =
    { PostId: string
      Title: string
      CreatedAt: DateTimeOffset }

type State =
    { Posts: PostSummary list
      Loading: bool
      Error: string option }

type Msg =
    | ListPostsRequested
    | ListPostsFailed of string
    | PostsReceived of PostSummary list

let init () =
    { Posts =
        [
            { PostId = "first"; Title = "First Post"; CreatedAt = DateTimeOffset.UtcNow }
            { PostId = "second"; Title = "Second Post"; CreatedAt = DateTimeOffset.UtcNow }
        ]
      Loading = true
      Error = None }

let update (msg:Msg) (state:State): State =
    match msg with
    | ListPostsRequested -> { state with Loading = true }
    | ListPostsFailed error -> { state with Error = Some error }
    | PostsReceived posts -> { state with Posts = posts }

let render (state:State) (dispatch:Msg -> unit) =
    let posts =
        state.Posts |> List.map (fun post ->
            Mui.listItem [
                prop.key post.PostId
                listItem.children [
                    Mui.listItemText [
                        listItemText.primary post.Title
                        listItemText.secondary (post.CreatedAt.ToString())
                    ]
                ]
            ]
        )
    Mui.list posts
