[<RequireQualifiedAccess>]
module Client.Page.Home

open Client
open Client.UseBlog
open Feliz
open Feliz.Router
open Shared.Domain

[<ReactComponent>]
let SkeletonListItem () =
    Html.div [
        prop.className "border-b-2 border-gray-200 p-2"
        prop.children [
            Html.div [
                prop.className "flex justify-between"
                prop.children [
                    Html.h3 [
                        prop.className "animate-pulse h-6 w-1/3 mb-2 bg-gray-200"
                    ]
                    Html.p [
                        prop.className "animate-pulse h-4 w-2/12 bg-gray-200"
                    ]
                ]
            ]
            Html.p [
                prop.className "animate-pulse h-4 w-8/12 bg-gray-200"
            ]
        ]
    ]
[<ReactComponent>]
let PostListItem (postSummary:PostSummary) =
    Html.div [
        prop.id postSummary.permalink
        prop.className "post-item border-b-2 border-gray-200 p-2 cursor-pointer hover:bg-gray-100"
        prop.onClick (fun _ -> Router.navigatePath $"/{postSummary.permalink}")
        prop.children [
            Html.div [
                prop.className "flex justify-between"
                prop.children [
                    Html.h3 [
                        prop.className "text-lg font-medium text-gray-800 mb-2"
                        prop.text postSummary.title
                    ]
                    Html.p [
                        prop.className "text-sm text-gray-500 leading-7"
                        prop.text (postSummary.updatedAt.ToString("MM/dd/yyyy"))
                    ]
                ]
            ]
            Html.p [
                prop.className "post-summary text-sm text-gray-500"
                prop.text (postSummary.summary |> Option.defaultValue "")
            ]
        ]
    ]
    
[<ReactComponent>]
let SkeletonList () =
    Html.div [
        prop.className "px-2"
        prop.children [
            for _ in 1..3 do
                SkeletonListItem()
        ]
    ]

[<ReactComponent>]
let PostList (postSummaries:PostSummary list) =
    Html.div [
        prop.className "px-2"
        prop.children [
            for postSummary in postSummaries do
                PostListItem postSummary
        ]
    ]

[<ReactComponent>]
let HomePage () =
    let blog = React.useBlog()
    match blog.State.Posts with
    | HasNotStarted
    | InProgress ->
        SkeletonList()
    | Resolved posts ->
        PostList posts
    | Error msg ->
        Html.h2 $"Error: {msg}"
