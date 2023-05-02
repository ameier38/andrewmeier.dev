module Server.Views.Articles

open Server.ViewEngine
open Server.Components
open Server.NotionClient
open type Html
open type Htmx
open type Alpine

let private articleSummary (page:PageProperties) =
    let url = $"/articles/{page.permalink}"
    article [
        _id page.permalink
        _class "md:grid md:grid-cols-4 md:items-baseline"
        _children [
            time [
                _class "hidden md:block flex items-center text-sm text-gray-400"
                _datetime (page.createdAt.ToString("yyyy-mm-dd"))
                _children (page.createdAt.ToString("MMMM d, yyyy"))
            ]
            div [
                _class "md:col-span-3 flex flex-col items-start"
                _children [
                    time [
                        _class "md:hidden border-l border-gray-300 pl-3 mb-3 flex items-center text-sm text-gray-400"
                        _datetime (page.createdAt.ToString("yyyy-mm-dd"))
                        _children (page.createdAt.ToString("MMMM d, yyyy"))
                    ]
                    a [
                        _href url
                        _hxGet url
                        _hxTarget "#page"
                        _hxIndicator "#page-loading"
                        _class "w-full p-4 sm:rounded-2xl hover:bg-gray-100"
                        _children [
                            h2 [
                                _class "text-base font-semibold tracking-tight text-gray-800"
                                _children page.title
                            ]
                            p [
                                _class "mt-2 text-sm text-gray-600"
                                _children page.summary
                            ]
                            div [
                                _class "mt-4 flex flex-wrap gap-2"
                                _children (page.tags |> Seq.map Tag.primary)
                            ]
                            div [
                                _class "mt-4 flex items-start space-x-1 text-sm font-medium text-emerald-500"
                                _children [
                                    raw "Read"
                                    Icon.chevronRight
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
        
let articlesPage (pages:PageProperties seq) =
    div [
        _xInit "selectedNav = 'Articles'; window.scrollTo({top: 0, behavior: 'instant'})"
        _class "mx-auto max-w-3xl"
        _children [
            header [
                _class "max-w-2xl"
                _children [
                    h1 [
                        _class "text-4xl text-gray-900 font-medium"
                        _children "Andrew's Thoughts"
                    ]
                    p [
                        _class "mt-6 text-base text-gray-600"
                        _children "My thoughts on a variety of topics"
                    ]
                ]
            ]
            div [
                _class "mt-16"
                _children [
                    div [
                        _class "md:border-l md:border-gray-300 md:pl-6"
                        _children [
                            div [
                                _class "max-w-3xl flex flex-col space-y-4"
                                _children (pages |> Seq.map articleSummary)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
    
let articlePage (detail:PageDetail) =
        div [
            _xInit "selectedNav = 'Articles'; window.scrollTo({top: 0, behavior: 'instant'})"
            _class "mx-auto max-w-3xl"
            _children [
                article [
                    _children [
                        header [
                            _class "flex flex-col"
                            _children [
                                time [
                                    _class "text-base text-gray-400 border-l border-gray-300 pl-2"
                                    _datetime (detail.properties.createdAt.ToString("yyyy-mm-dd"))
                                    _children (detail.properties.createdAt.ToString("MMMM d, yyyy"))
                                ]
                                h1 [
                                    _class "mt-4 text-4xl font-bold tracking-tight text-gray-900"
                                    _children detail.properties.title
                                ]
                            ]
                        ]
                        div [
                            _class "mt-8 prose max-w-none"
                            _xInit "Prism.highlightAllUnder($el)"
                            _children (Content.toHtml detail.content)
                        ]
                    ]
                ]
            ]
        ]
        
let notFoundPage =
    div [
        _class "flex flex-col items-center"
        _children [
            h1 [
                _class "text-3xl text-gray-800"
                _children "Oops! Could not find page."
            ]
            p [
                _class "text-md text-gray-600"
                _children "Something went wrong. Try refreshing the page."
            ]
        ]
    ]
