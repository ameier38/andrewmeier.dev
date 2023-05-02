module Server.Views.Projects

open Server.ViewEngine
open Server.Components
open type Html
open type Htmx
open type Alpine

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
        _xInit "selectedNav = 'Projects'"
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
