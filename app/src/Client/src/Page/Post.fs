[<RequireQualifiedAccess>]
module Client.Page.Post

open Client
open Client.UseBlog
open Feliz
open Shared.Domain

[<ReactComponent>]
let Post (post: Post) =
    let highlight (el:Browser.Types.Element) =
        match el with
        | null -> ()
        | el -> Prism.highlightAllUnder el
    let ref = React.useCallback(highlight)
    Html.div [
        prop.className "flex justify-center"
        prop.children [
            Html.div [
                prop.className "prose max-w-full"
                prop.children [
                    Html.h1 [
                        prop.id "title"
                        prop.text post.Title
                    ]
                    Html.article [
                        prop.ref ref
                        prop.className "post"
                        prop.dangerouslySetInnerHTML post.Content
                    ]
                ]
            ]
        ]
    ]
    
[<ReactComponent>]
let PostPage (permalink:string) =
    let blog = React.useBlog()
    match blog.State.SelectedPost with
    | HasNotStarted ->
        blog.loadPost permalink
        Html.h2 "Loading..."
    | Resolved { Permalink = currentPermalink } when currentPermalink <> permalink ->
        blog.loadPost permalink
        Html.h2 "Loading..."
    | InProgress ->
        Html.h2 "Loading..."
    | Resolved post ->
        Post post
    | Error err ->
        Html.h2 $"Error: {err}"
