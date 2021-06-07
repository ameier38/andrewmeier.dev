[<RequireQualifiedAccess>]
module Client.Page.Home

open Client
open Client.UseBlog
open Feliz
open Feliz.Router
open Shared.Domain

[<ReactComponent>]
let PostListItem (postSummary:PostSummary) =
    Html.div [
        prop.id postSummary.Permalink
        prop.className "post-item border-b-2 border-gray-200 p-2 cursor-pointer hover:bg-gray-100"
        prop.onClick (fun _ -> Router.navigatePath $"/{postSummary.Permalink}")
        prop.children [
            Html.div [
                prop.className "flex justify-between"
                prop.children [
                    Html.h3 [
                        prop.className "text-lg leading-8 font-medium text-gray-900"
                        prop.text postSummary.Title
                    ]
                    Html.p [
                        prop.className "mt-1 text-sm text-gray-500"
                        prop.text (postSummary.UpdatedAt.ToString("MM/dd/yyyy"))
                    ]
                ]
            ]
            Html.p [
                prop.className "post-summary text-sm text-gray-500"
                prop.text postSummary.Summary
            ]
        ]
    ]
    
[<ReactComponent>]
let PostList (postSummaries:PostSummary list) =
    Html.div [
        for postSummary in postSummaries do
            PostListItem postSummary
    ]

[<ReactComponent>]
let HomePage () =
    let blog = React.useBlog()
    match blog.State.Posts with
    | HasNotStarted
    | InProgress ->
        Html.h2 "Loading..."
    | Error msg ->
        Html.h2 $"Error: {msg}"
    | Resolved posts ->
        PostList posts
