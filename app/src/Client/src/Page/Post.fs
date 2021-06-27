[<RequireQualifiedAccess>]
module Client.Page.Post

open Client
open Client.UseBlog
open Feliz
open Shared.Domain

[<ReactComponent>]
let Post (post: Post) =
    let onMount (el:Browser.Types.Element) =
        match el with
        | null -> ()
        | el -> Prism.highlightAllUnder el
    let ref = React.useCallback(onMount)
    Html.div [
        prop.className "w-full"
        prop.children [
            Html.div [
                prop.className "relative h-40 bg-blend-overlay bg-gray-800 mb-2"
                prop.style [
                    style.backgroundImageUrl post.Cover
                ]
                prop.children [
                    Html.div [
                        prop.className "absolute top-0 left-0 px-2"
                        prop.children [
                            Html.h1 [
                                prop.className "text-3xl text-gray-200 font-bold mt-2 mb-2"
                                prop.id "title"
                                prop.text post.Title
                            ]
                            Html.p [
                                let createdAt = post.CreatedAt.ToString("M/d/yyyy")
                                prop.className "text-gray-400"
                                prop.text $"Created: {createdAt}"
                            ]
                            Html.p [
                                let updatedAt = post.UpdatedAt.ToString("M/d/yyyy")
                                prop.className "text-gray-400"
                                prop.text $"Updated: {updatedAt}"
                            ]
                        ]
                    ]
                ]
            ]
            Html.article [
                prop.ref ref
                prop.className "post prose px-2 max-w-none"
                prop.dangerouslySetInnerHTML post.Content
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
