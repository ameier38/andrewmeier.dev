[<RequireQualifiedAccess>]
module Client.Page.Post

open Client
open Client.UseBlog
open Feliz
open Shared.Domain

[<ReactComponent>]
let Skeleton () =
    Html.div [
        prop.className "w-full"
        prop.children [
            Html.div [
                prop.className "bg-blend-overlay bg-gray-600 mb-2"
                prop.children [
                    Html.div [
                        prop.className "p-2"
                        prop.children [
                            Html.h1 [
                                prop.className "animate-pulse rounded h-6 w-3/4 mb-10 bg-gray-200"
                            ]
                            Html.p [
                                prop.className "animate-pulse rounded h-4 w-1/3 mb-1 bg-gray-200"
                            ]
                            Html.p [
                                prop.className "animate-pulse rounded h-4 w-1/3 mb-1 bg-gray-200"
                            ]
                        ]
                    ]
                ]
            ]
            Html.div [
                prop.className "px-2"
                prop.children [
                    Html.p [
                        prop.className "animate-pulse rounded h-4 w-10/12 mb-1 bg-gray-200"
                    ]
                    Html.p [
                        prop.className "animate-pulse rounded h-4 w-11/12 mb-1 bg-gray-200"
                    ]
                    Html.p [
                        prop.className "animate-pulse rounded h-4 w-9/12 mb-1 bg-gray-200"
                    ]
                    Html.p [
                        prop.className "animate-pulse rounded h-4 w-10/12 mb-1 bg-gray-200"
                    ]
                    Html.p [
                        prop.className "animate-pulse rounded h-4 w-11/12 mb-1 bg-gray-200"
                    ]
                ]
            ]
        ]
    ]

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
                prop.className "bg-blend-overlay bg-gray-800 mb-2"
                prop.style [
                    style.backgroundImageUrl post.cover
                ]
                prop.children [
                    Html.div [
                        prop.className "p-2"
                        prop.children [
                            Html.h1 [
                                prop.className "text-3xl text-gray-200 font-bold mb-10"
                                prop.id "title"
                                prop.text post.title
                            ]
                            Html.p [
                                let createdAt = post.createdAt.ToString("M/d/yyyy")
                                prop.className "text-gray-400"
                                prop.text $"Created: {createdAt}"
                            ]
                            Html.p [
                                let updatedAt = post.updatedAt.ToString("M/d/yyyy")
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
                prop.dangerouslySetInnerHTML post.content
            ]
        ]
    ]
    
[<ReactComponent>]
let PostPage (permalink:string) =
    let blog = React.useBlog()
    match blog.State.SelectedPost with
    | HasNotStarted ->
        blog.loadPost permalink
        Skeleton()
    | Resolved { permalink = currentPermalink } when currentPermalink <> permalink ->
        blog.loadPost permalink
        Skeleton()
    | InProgress ->
        Skeleton()
    | Resolved post ->
//        Skeleton()
        Post post
    | Error err ->
        Html.h2 $"Error: {err}"
