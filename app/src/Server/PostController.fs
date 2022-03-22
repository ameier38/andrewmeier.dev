namespace Server.Controllers

open Microsoft.AspNetCore.Mvc
open Notion.Client
open Feliz.ViewEngine
open Server.PostClient
open Serilog

module Icon =
    let private icon (path:string) =
        Html.svg [
            prop.className "h-8 w-8 p-1 fill-current"
            prop.custom ("viewbox", "0 0 16 16")
            prop.children [
                Html.path [
                    prop.custom ("d", path)
                ]
            ]
        ]
    let GitHub = icon "M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.012 8.012 0 0 0 16 8c0-4.42-3.58-8-8-8z"
    let Twitter = icon "M5.026 15c6.038 0 9.341-5.003 9.341-9.334 0-.14 0-.282-.006-.422A6.685 6.685 0 0 0 16 3.542a6.658 6.658 0 0 1-1.889.518 3.301 3.301 0 0 0 1.447-1.817 6.533 6.533 0 0 1-2.087.793A3.286 3.286 0 0 0 7.875 6.03a9.325 9.325 0 0 1-6.767-3.429 3.289 3.289 0 0 0 1.018 4.382A3.323 3.323 0 0 1 .64 6.575v.045a3.288 3.288 0 0 0 2.632 3.218 3.203 3.203 0 0 1-.865.115 3.23 3.23 0 0 1-.614-.057 3.283 3.283 0 0 0 3.067 2.277A6.588 6.588 0 0 1 .78 13.58a6.32 6.32 0 0 1-.78-.045A9.344 9.344 0 0 0 5.026 15z"
    
module Spinner =
    let circle =
        Html.svg [
            prop.className "animate-spin -ml-1 mr-3 h-5 w-5 text-white"
            prop.custom ("viewbox", "0 0 24 24")
            prop.fill "none"
            prop.children [
                Html.circle [
                    prop.className "opacity-25"
                    prop.cx 12
                    prop.cy 12
                    prop.r 10
                    prop.stroke "currentColor"
                    prop.strokeWidth 4
                ]
                Html.path [
                    prop.className "opacity-75"
                    prop.fill "currentColor"
                    prop.custom ("d", "M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z")
                ]
            ]
        ]
        
module RichTextBase =
    let toHtml (text:RichTextBase) =
        let inner =
            if text.Annotations.IsCode then
                Html.code [
                    prop.className "language-none"
                    prop.text text.PlainText
                ]
            else
                let classes = [
                    if text.Annotations.IsBold then "font-bold"
                    if text.Annotations.IsItalic then "italic"
                    if text.Annotations.IsUnderline then "underline"
                    if text.Annotations.IsStrikeThrough then "line-through"
                ]
                Html.text [
                    prop.classes classes
                    prop.text text.PlainText
                ]
        if isNull text.Href then inner
        else
            Html.a [
                prop.href text.Href
                prop.children inner
            ]
    
module Block =
    let (|Bulleted|Numbered|Other|) (block:IBlock) =
        match block with
        | :? BulletedListItemBlock -> Bulleted
        | :? NumberedListItemBlock -> Numbered
        | _ -> Other
    let isNumberedListItem (block:IBlock) =
        match block with
        | :? NumberedListItemBlock -> true
        | _ -> false
    let rec toHtml (block:IBlock) =
        match block with
        | :? HeadingOneBlock as b ->
            Html.h1 [
                prop.id (b.Id.Replace("-", ""))
                prop.className "text-3xl font-medium text-gray-800"
                prop.children (b.Heading_1.Text |> Seq.map RichTextBase.toHtml)
            ]
        | :? HeadingTwoBlock as b ->
            Html.h2 [
                prop.id (b.Id.Replace("-", ""))
                prop.className "text-2xl font-medium text-gray-800"
                prop.children (b.Heading_2.Text |> Seq.map RichTextBase.toHtml)
            ]
        | :? HeadingThreeeBlock as b ->
            Html.h3 [
                prop.id (b.Id.Replace("-", ""))
                prop.className "text-xl font-medium text-gray-800"
                prop.children (b.Heading_3.Text |> Seq.map RichTextBase.toHtml)
            ]
        | :? ParagraphBlock as b ->
            Html.div [
                prop.className "text-gray-800"
                prop.children [
                    if Seq.isEmpty b.Paragraph.Text then
                        Html.br []
                    else
                        for text in b.Paragraph.Text do
                            RichTextBase.toHtml text
                        if b.HasChildren then
                            Html.div [
                                prop.className "indent-1"
                                prop.children [
                                    for child in b.Paragraph.Children do
                                        toHtml child
                                ]
                            ]
                ]
            ]
        | :? BulletedListItemBlock as b ->
            Html.li [
                for text in b.BulletedListItem.Text do
                    RichTextBase.toHtml text
                if b.HasChildren then
                    Html.div [
                        prop.className "indent-1"
                        prop.children [
                            for child in b.BulletedListItem.Children do
                                toHtml child
                        ]
                    ]
            ]
        | :? NumberedListItemBlock as b ->
            Html.li [
                for text in b.NumberedListItem.Text do
                    RichTextBase.toHtml text
                if b.HasChildren then
                    Html.div [
                        prop.className "indent-1"
                        prop.children [
                            for child in b.NumberedListItem.Children do
                                toHtml child
                        ]
                    ]
            ]
        | :? CodeBlock as b ->
            Html.pre [
                prop.className $"language-{b.Code.Language}"
                prop.children [
                    Html.code [
                        prop.className $"language-{b.Code.Language}"
                        prop.children [
                            for text in b.Code.Text do
                                RichTextBase.toHtml text
                        ]
                    ]
                ]
            ]
        | :? ImageBlock as b ->
            let url =
                match b.Image with
                | :? UploadedFile as f -> f.File.Url
                | :? ExternalFile as f -> f.External.Url
                | _ -> ""
            Html.img [
                prop.className "drop-shadow-xl rounded"
                prop.src url
            ]
        | :? DividerBlock ->
            Html.div [ prop.className "border-b-2 border-gray-300" ]
        | :? QuoteBlock as b ->
            Html.blockquote [
                for text in b.Quote.Text do
                    RichTextBase.toHtml text
                if b.HasChildren then
                    Html.div [
                        prop.className "indent-1"
                        prop.children [
                            for child in b.Quote.Children do
                                toHtml child
                        ]
                    ]
            ]
        | other ->
            Log.Warning("Unsupported block {Block}", other)
            Html.none
            
module Content =
    let toHtml (blocks:IBlock[]) =
        let elements = ResizeArray()
        let bulletedListItems = ResizeArray()
        let numberedListItems = ResizeArray()
        let flushBulletedListItems () =
            let children = bulletedListItems.ToArray()
            let ul = Html.ul [
                prop.className "list-disc"
                prop.children children
            ]
            elements.Add(ul)
            bulletedListItems.Clear()
        let flushNumberedListItems () =
            let children = numberedListItems.ToArray()
            let ol = Html.ol [
                prop.className "list-decimal"
                prop.children children
            ]
            elements.Add(ol)
            numberedListItems.Clear()
        for block in blocks do
            match block with
            | Block.Bulleted ->
                if numberedListItems.Count > 0 then flushNumberedListItems()
                bulletedListItems.Add(Block.toHtml block)
            | Block.Numbered ->
                if bulletedListItems.Count > 0 then flushBulletedListItems()
                numberedListItems.Add(Block.toHtml block)
            | Block.Other ->
                if numberedListItems.Count > 0 then flushNumberedListItems()
                if bulletedListItems.Count > 0 then flushBulletedListItems()
                elements.Add(Block.toHtml block)
        elements.ToArray()
    
module Components =
    let layout (page:ReactElement) =
        Html.html [
            Html.head [
                Html.title "Andrew Meier"
                Html.meta [ prop.charset.utf8 ]
                Html.meta [
                    prop.name "viewport"
                    prop.content "width=device-width, initial-scale=1.0"
                ]
                Html.link [
                    prop.href "/css/compiled.css"
                    prop.rel "stylesheet"
                ]
                Html.link [
                    prop.href "/css/prism.css"
                    prop.rel "stylesheet"
                ]
                Html.script [ prop.src "https://unpkg.com/htmx.org@1.7.0" ]
            ]
            Html.body [
                prop.className "bg-gray-50"
                prop.children [
                    Html.nav [
                        prop.className "h-16 bg-gray-100 border-b-2 border-gray-200"
                        prop.children [
                            Html.div [
                                prop.className "container mx-auto max-w-3xl h-full flex justify-between items-center text-gray-800"
                                prop.children [
                                    Html.a [
                                        prop.className "p-2 rounded text-lg font-medium cursor-pointer hover:bg-gray-200"
                                        prop.href "/"
                                        prop.text "Andrew's Thoughts"
                                    ]
                                    Html.div [
                                        prop.className "flex items-center space-x-2"
                                        prop.children [
                                            Html.a [
                                                prop.className "p-2 rounded text-md font-medium cursor-pointer hover:bg-gray-200"
                                                prop.href "/about"
                                                prop.text "About"
                                            ]
                                            Html.a [
                                                prop.className "rounded-full cursor-pointer hover:bg-gray-200"
                                                prop.href "https://twitter.com/ameier38"
                                                prop.children Icon.Twitter
                                            ]
                                            Html.a [
                                                prop.className "rounded-full cursor-pointer hover:bg-gray-200"
                                                prop.href "https://github.com/ameier38"
                                                prop.children Icon.GitHub
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                    Html.div [
                        prop.id "page"
                        prop.children page
                    ]    
                ]
            ]
            Html.script [
                prop.src "/scripts/prism.js"
                prop.custom ("data-manual", true)
            ]
        ]
        
    let postSummary (post:Post) =
        Html.div [
            prop.id post.permalink
            prop.className "post relative border-b-2 border-gray-200 p-2 cursor-pointer hover:bg-gray-100"
            prop.custom ("hx-get", $"/partial/{post.id}")
            prop.custom ("hx-target", "#page")
            prop.custom ("hx-swap", "outerHTML")
            prop.custom ("hx-push-url", $"/{post.permalink}")
            prop.children [
                Html.div [
                    Html.div [
                        prop.className "flex justify-between"
                        prop.children [
                            Html.h3 [
                                prop.className "text-lg font-medium text-gray-800 mb-2"
                                prop.text post.title
                            ]
                            Html.p [
                                prop.className "text-sm text-gray-500 leading-7"
                                prop.text (post.updatedAt.ToString("MM/dd/yyyy"))
                            ]
                        ]
                    ]
                    Html.p [
                        prop.className "post-summary text-sm text-gray-500"
                        prop.text post.summary
                    ]
                ]
                Html.div [
                    prop.className "loader absolute inset-0 w-full h-full bg-gray-500/25"
                    prop.children [
                        Html.div [
                            prop.className "flex justify-center items-center h-full"
                            prop.children Spinner.circle
                        ]
                    ]
                ]
            ]
        ]
        
    let postList (posts:Post[]) =
        Html.div [
            prop.className "container mx-auto max-w-3xl"
            prop.children [
                for post in posts do
                    postSummary post
            ]
        ]
        
    let postDetail (detail:PostDetail) =
        Html.div [
            prop.className "w-full"
            prop.children [
                Html.div [
                    prop.className "bg-no-repeat bg-center bg-cover bg-blend-overlay bg-gray-800 mb-2"
                    prop.style [
                        style.backgroundImageUrl detail.post.cover
                        
                    ]
                    prop.children [
                        Html.div [
                            prop.className "container mx-auto max-w-3xl p-2"
                            prop.children [
                                Html.h1 [
                                    prop.className "text-3xl text-gray-200 font-bold mb-14"
                                    prop.id "title"
                                    prop.text detail.post.title
                                ]
                                Html.p [
                                    let createdAt = detail.post.createdAt.ToString("M/d/yyyy")
                                    prop.className "text-gray-400"
                                    prop.text $"Created: {createdAt}"
                                ]
                                Html.p [
                                    let updatedAt = detail.post.updatedAt.ToString("M/d/yyyy")
                                    prop.className "text-gray-400"
                                    prop.text $"Updated: {updatedAt}"
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "container mx-auto max-w-3xl px-2"
                    prop.children [
                        Html.article [
                            prop.id "post"
                            prop.className "prose max-w-none mb-8"
                            prop.children (Content.toHtml detail.content)
                        ]
                    ]
                ]
                Html.script [ prop.src "/scripts/highlight.js" ]
            ]
        ]
        
    let notFound = Html.div [
        prop.className "flex justify-center"
        prop.children [
            Html.div [
                prop.className "flex flex-col items-center pt-4"
                prop.children [
                    Html.h1 [
                        prop.className "text-3xl font-medium text-gray-800 mb-4"
                        prop.text "Page not found"
                    ]
                    Html.a [
                        prop.className "rounded p-2 bg-gray-50 text-gray-800 border-gray-800 border hover:bg-gray-100 cursor-pointer"
                        prop.href "/"
                        prop.text "Back to home"
                    ]
                ]
            ]
        ]
    ]
    

[<Route("")>]
type PostController(client:IPostClient) =
    inherit Controller()
    
    member private this.Render(html: ReactElement) =
        let htmlContent = Render.htmlView html
        this.Content(htmlContent, "text/html")
        
    [<Route("partial/{postId:guid}")>]
    [<HttpGet>]
    member this.PartialPage(postId:string) = async {
        match! client.GetById(postId) with
        | Some post ->
            let html = Components.postDetail post
            return this.Render(html)
        | None ->
            let page = Components.notFound
            let html = Components.layout page
            return this.Render(html)
    }
        
    [<HttpGet>]
    member this.Index() = async {
        let! posts = client.List()
        let page = Components.postList posts
        let html = Components.layout page
        return this.Render(html)
    }
        
    [<Route("{postId:guid}")>]
    [<HttpGet>]
    member this.PostById(postId:string) = async {
        match! client.GetById(postId) with
        | Some post ->
            let page = Components.postDetail post
            let html = Components.layout page
            return this.Render(html)
        | None ->
            let page = Components.notFound
            let html = Components.layout page
            return this.Render(html)
    }
    
    [<Route("{permalink:regex(^[[a-z-]]+$)}")>]
    [<HttpGet>]
    member this.PostByPermalink(permalink:string) = async {
        match! client.GetByPermalink(permalink) with
        | Some post ->
            let page = Components.postDetail post
            let html = Components.layout page
            return this.Render(html)
        | None ->
            let page = Components.notFound
            let html = Components.layout page
            return this.Render(html)
    }
