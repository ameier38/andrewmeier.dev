module Server.Views.Index

open Server.ViewEngine
open Server.NotionClient
open Server.Components
open type Html
open type Alpine

let indexPage (page:PageDetail) =
    div [
        _xInit "selectedNav = 'About'"
        _children [
            div [
                _class "grid gap-4 grid-cols-1 md:grid-cols-2"
                _children [
                    div [
                        _class "flex flex-col items-center"
                        _children [
                            img [
                                _class "max-w-xs rounded-full mb-4"
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
                                _class "text-2xl text-gray-800 font-medium"
                                _children page.properties.title
                            ]
                            div [
                                _class "prose"
                                _children (Content.toHtml page.content)
                            ]
                        ]
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
                _children "Oops!"
            ]
            p [
                _class "text-md text-gray-600"
                _children "Something went wrong. Try refreshing the page."
            ]
        ]
    ]