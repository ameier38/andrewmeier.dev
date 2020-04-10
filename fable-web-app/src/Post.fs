[<RequireQualifiedAccess>]
module Blog.Post

open System
open Feliz
open Feliz.MaterialUI

type Url =
    | EmptyUrl
    | PostUrl of postId:string

type Post =
    { PostId: string
      Title: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      Cover: string
      Content: string }

type State =
    { CurrentUrl: Url
      Post: Post option
      Loading: bool }

type Msg =
    | UrlChanged of Url

let init (url:Url) =
    match url with
    | EmptyUrl ->
        { CurrentUrl = url
          Post = None
          Loading = true }
    | PostUrl postId ->
        { CurrentUrl = url
          Post =
            { PostId = postId
              Title = sprintf "Test %s" postId
              CreatedAt = DateTimeOffset.UtcNow
              UpdatedAt = DateTimeOffset.UtcNow
              Cover = "test"
              Content = "# Hello\ntesting"}
            |> Some
          Loading = false }

let update (msg:Msg) (state:State) =
    printfn "received msg: %A" msg
    match msg with
    | UrlChanged EmptyUrl -> state
    | UrlChanged (PostUrl newPostId) ->
        match state.CurrentUrl with
        // don't refetch if the post is the same as current state
        // | PostUrl prevPostId when prevPostId = newPostId -> state
        | EmptyUrl
        | PostUrl _ ->
            { state with
                Post =
                    { PostId = newPostId
                      Title = sprintf "Test %s" newPostId
                      CreatedAt = DateTimeOffset.UtcNow
                      UpdatedAt = DateTimeOffset.UtcNow
                      Cover = "test"
                      Content = "# Hello\ntesting"}
                    |> Some
                Loading = false }

let render (state:State) (dispatch:Msg -> unit) =
    match state.Loading, state.Post with
    | true, _
    | false, None ->
        Mui.card [
            Mui.skeleton [
                prop.height 100
                skeleton.variant.rect
            ]
            Mui.skeleton [
                skeleton.animation.pulse
                skeleton.width 60
            ]
            Mui.skeleton [
                skeleton.animation.pulse
            ]
        ]
    | false, Some post ->
        Mui.card [
            Mui.cardMedia [
                prop.height 100
                cardMedia.src post.Cover
            ]
            Mui.cardContent [
                Mui.typography [
                    typography.variant.h6
                    typography.gutterBottom true
                    typography.children [
                        post.Title
                    ]
                ]
                Mui.typography [
                    typography.variant.subtitle1
                    typography.children [
                        post.CreatedAt.ToString()
                    ]
                ]
            ]
        ]
