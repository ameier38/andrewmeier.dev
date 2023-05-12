module Server.Views

open FSharp.ViewEngine
open Server.Components
open Server.NotionClient
open type Html
open type Htmx
open type Alpine

let private postSummary (page:PageProperties) =
    let url = $"/{page.permalink}"
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
        
let postsPage (pages:PageProperties seq) =
    div [
        _xInit "selectedNav = 'Posts'; window.scrollTo({top: 0, behavior: 'instant'})"
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
                                _children (pages |> Seq.map postSummary)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
    
let postPage (detail:PageDetail) =
        div [
            _xInit "selectedNav = 'Posts'; window.scrollTo({top: 0, behavior: 'instant'})"
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
        
let projectCard (image:string) (title:string) (summary:string) (url:string) =
    li [
        _class "flex flex-col items-start rounded-2xl"
        _children [
            a [
                _class "p-4 rounded-2xl group hover:bg-gray-100"
                _href url
                _children [
                    div [
                        _class "flex w-12 h-12 justify-center items-center rounded-full bg-white shadow-md shadow-gray-800/5 ring-1 ring-gray-900/5"
                        _children [
                            img [
                                _class "h-8 w-8"
                                _src image
                            ]
                        ]
                    ]
                    h2 [
                        _class "mt-4 text-base font-semibold text-gray-800"
                        _children title
                    ]
                    p [
                        _class "mt-2 text-sm text-gray-600"
                        _children summary
                    ]
                    p [
                        _class "mt-4 flex items-center space-x-2 text-sm font-medium text-gray-400 group-hover:text-emerald-500"
                        _children [
                            Icon.link
                            span [ _children url ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let projectsPage =
    div [
        _xInit "selectedNav = 'Projects'; window.scrollTo({top: 0, behavior: 'instant'})"
        _class "mx-auto max-w-3xl"
        _children [
            header [
                _class "max-w-2xl"
                _children [
                    h1 [
                        _class "text-4xl text-gray-900 font-medium"
                        _children "Projects I Am Working On"
                    ]
                    p [
                        _class "mt-6 text-base text-gray-600"
                        _children "These projects are solutions to problems that I have come across in my life."
                    ]
                ]
            ]
            ul [
                _class "mt-16 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-12"
                _children [
                    projectCard "/images/meiermade.svg" "Meier Made, LLC" "Consulting services" "https://meiermade.com" 
                ]
            ]
        ]
    ]
    
let aboutPage (page:PageDetail) =
    div [
        _xInit "selectedNav = 'About'; window.scrollTo({top: 0, behavior: 'instant'})"
        _children [
            div [
                _class "grid gap-4 grid-cols-1 md:grid-cols-2"
                _children [
                    div [
                        _class "flex flex-col items-center"
                        _children [
                            img [
                                _class "w-72 aspect-square rounded-full mb-4"
                                _src page.properties.icon
                            ]
                            div [
                                _class "flex justify-center space-x-2"
                                _children [
                                    a [
                                        _class "p-2 text-gray-600 rounded-full hover:bg-gray-100"
                                        _href "https://github.com/ameier38"
                                        _children Icon.github
                                    ]
                                    a [
                                        _class "p-2 text-gray-600 rounded-full hover:bg-gray-100"
                                        _href "https://twitter.com/ameier38"
                                        _children Icon.twitter
                                    ]
                                    a [
                                        _class "p-2 text-gray-600 rounded-full hover:bg-gray-100"
                                        _href "https://www.linkedin.com/in/andrew-meier/"
                                        _children Icon.linkedIn
                                    ]
                                ]
                            ]
                        ]
                    ]
                    div [
                        _class "px-2 md:order-first"
                        _children [
                            h1 [
                                _class "text-2xl text-gray-900 font-medium"
                                _children page.properties.title
                            ]
                            div [
                                _class "mt-6 prose"
                                _children (Content.toHtml page.content)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
