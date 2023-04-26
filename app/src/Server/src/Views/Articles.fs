module Server.Views.Articles

open Server.ViewEngine
open Server.Components
open Server.NotionClient
open type Html
open type Htmx
open type Alpine

let private articleSummary (page:PageProperties) =
    div [
        _id page.permalink
        _class "relative border-b-2 border-gray-200 p-2 cursor-pointer hover:bg-gray-100"
        // i.e., when we click this div, make a GET request to /<post-id>
        _hxGet $"/articles/{page.id}"
        // i.e., take the response from the above GET request and replace the element with id 'page'
        _hxTarget "#page"
        _children [
            div [
                _class "flex justify-between"
                _children [
                    h3 [
                        _class "text-lg font-medium text-gray-800 mb-2"
                        _children page.title
                    ]
                    p [
                        _class "text-sm text-gray-500 leading-7"
                        _children (page.createdAt.ToString("MM/dd/yyyy"))
                    ]
                ]
            ]
            p [
                _class "text-sm text-gray-500"
                _children page.summary
            ]
        ]
    ]
        
let articlesPage (pages:PageProperties seq) =
    div [
        _class "container mx-auto max-w-3xl"
        _children (pages |> Seq.map articleSummary)
    ]
    
let articlePage (detail:PageDetail) =
        div [
            _class "w-full"
            _children [
                div [
                    _class "bg-no-repeat bg-center bg-cover bg-blend-overlay bg-gray-800 mb-2"
                    _style $"background-image: url('{detail.properties.cover}')"
                    _children [
                        div [
                            _class "container mx-auto max-w-3xl p-2"
                            _children [
                                h1 [
                                    _id "title"
                                    _class "text-3xl text-gray-200 font-bold mb-14"
                                    _children detail.properties.title
                                ]
                                p [
                                    let createdAt = detail.properties.createdAt.ToString("M/d/yyyy")
                                    _class "text-gray-400"
                                    _children $"Created: {createdAt}"
                                ]
                                p [
                                    let updatedAt = detail.properties.updatedAt.ToString("M/d/yyyy")
                                    _class "text-gray-400"
                                    _children $"Updated: {updatedAt}"
                                ]
                            ]
                        ]
                    ]
                ]
                div [
                    _class "container mx-auto max-w-3xl px-2"
                    _children [
                        div [
                            _id "post"
                            _class "prose max-w-none mb-8"
                            _xInit "Prism.highlightAllUnder($el)"
                            _children (Content.toHtml detail.content)
                        ]
                    ]
                ]
            ]
        ]
        
    
