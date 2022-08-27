namespace Server.Controllers

open Microsoft.AspNetCore.Mvc
open Notion.Client
open Server.PostClient
open Server.ViewEngine
open Serilog

open type Html
open type Svg
open type Html
open type Htmx
open type Hyper

module Icon =
    let github =
        svg [
            _height 24
            _width 24
            _fill "currentColor"
            _children [
                path [
                    _fillRule "evenodd"
                    _clipRule "evenodd"
                    _d "M12 2C6.477 2 2 6.463 2 11.97c0 4.404 2.865 8.14 6.839 9.458.5.092.682-.216.682-.48 0-.236-.008-.864-.013-1.695-2.782.602-3.369-1.337-3.369-1.337-.454-1.151-1.11-1.458-1.11-1.458-.908-.618.069-.606.069-.606 1.003.07 1.531 1.027 1.531 1.027.892 1.524 2.341 1.084 2.91.828.092-.643.35-1.083.636-1.332-2.22-.251-4.555-1.107-4.555-4.927 0-1.088.39-1.979 1.029-2.675-.103-.252-.446-1.266.098-2.638 0 0 .84-.268 2.75 1.022A9.606 9.606 0 0112 6.82c.85.004 1.705.114 2.504.336 1.909-1.29 2.747-1.022 2.747-1.022.546 1.372.202 2.386.1 2.638.64.696 1.028 1.587 1.028 2.675 0 3.83-2.339 4.673-4.566 4.92.359.307.678.915.678 1.846 0 1.332-.012 2.407-.012 2.734 0 .267.18.577.688.48C19.137 20.107 22 16.373 22 11.969 22 6.463 17.522 2 12 2z"
                ]
            ]
        ]
        
    let twitter =
        svg [
            _height 20
            _width 20
            _fill "currentColor"
            _children [
                path [
                    _d "M6.29 18.251c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0020 3.92a8.19 8.19 0 01-2.357.646 4.118 4.118 0 001.804-2.27 8.224 8.224 0 01-2.605.996 4.107 4.107 0 00-6.993 3.743 11.65 11.65 0 01-8.457-4.287 4.106 4.106 0 001.27 5.477A4.073 4.073 0 01.8 7.713v.052a4.105 4.105 0 003.292 4.022 4.095 4.095 0 01-1.853.07 4.108 4.108 0 003.834 2.85A8.233 8.233 0 010 16.407a11.616 11.616 0 006.29 1.84"
                ]
            ]
        ]
    
module Spinner =
    let circle =
        svg [
            _class "animate-spin -ml-1 mr-3 h-5 w-5 text-white"
            _viewBox "0 0 24 24"
            _fill "none"
            _children [
                circle [
                    _class "opacity-25"
                    _cx 12
                    _cy 12
                    _r 10
                    _stroke "currentColor"
                    _strokeWidth 4
                ]
                path [
                    _class "opacity-75"
                    _fill "currentColor"
                    _d "M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                ]
            ]
        ]
        
module RichTextBase =
    let toHtml (text:RichTextBase) =
        let inner =
            if text.Annotations.IsCode then
                code [
                    _class "language-none"
                    _children text.PlainText
                ]
            else
                span [
                    _class [
                        if text.Annotations.IsBold then "font-bold"
                        if text.Annotations.IsItalic then "italic"
                        if text.Annotations.IsUnderline then "underline"
                        if text.Annotations.IsStrikeThrough then "line-through"
                    ]
                    _children text.PlainText
                ]
        if isNull text.Href then inner
        else
            a [
                _href text.Href
                _children inner
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
            h1 [
                _class "mt-8"
                _id (b.Id.Replace("-", ""))
                _children (b.Heading_1.RichText |> Seq.map RichTextBase.toHtml |> Seq.toList)
            ]
        | :? HeadingTwoBlock as b ->
            h2 [
                _class "mt-6"
                _id (b.Id.Replace("-", ""))
                _children (b.Heading_2.RichText |> Seq.map RichTextBase.toHtml |> Seq.toList)
            ]
        | :? HeadingThreeeBlock as b ->
            h3 [
                _class "mt-4"
                _id (b.Id.Replace("-", ""))
                _children (b.Heading_3.RichText |> Seq.map RichTextBase.toHtml |> Seq.toList)
            ]
        | :? ParagraphBlock as b ->
            div [
                _children [
                    if Seq.isEmpty b.Paragraph.RichText then br
                    else
                        for text in b.Paragraph.RichText do
                            RichTextBase.toHtml text
                ]
            ]
        | :? BulletedListItemBlock as b ->
            li [
                _children [
                    for text in b.BulletedListItem.RichText do
                        RichTextBase.toHtml text
                ]
            ]
        | :? NumberedListItemBlock as b ->
            li [
                _children [
                    for text in b.NumberedListItem.RichText do
                        RichTextBase.toHtml text
                ]
            ]
        | :? CodeBlock as b ->
            let language =
                match b.Code.Language with
                | "f#" -> "fsharp"
                | "JSON" -> "json"
                | "TOML" -> "toml"
                | other -> other
            pre [
                _class $"language-{language}"
                _children [
                    code [
                        _class $"language-{language}"
                        _children [
                            for text in b.Code.RichText do
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
            img [
                _class "drop-shadow-xl rounded"
                _src url
            ]
        | :? DividerBlock ->
            div [ _class "border-b-2 border-gray-300" ]
        | :? QuoteBlock as b ->
            blockquote [
                _children [
                    for text in b.Quote.RichText do
                        RichTextBase.toHtml text
                ]
            ]
        | :? CalloutBlock as b ->
            div [
                _class "bg-gray-200 rounded p-2"
                _children [
                    for text in b.Callout.RichText do
                        RichTextBase.toHtml text
                ]
            ]
        | other ->
            Log.Warning("Unsupported block {Block}", other)
            empty
            
module Content =
    let toHtml (blocks:IBlock[]) =
        let elements = ResizeArray()
        let bulletedListItems = ResizeArray()
        let numberedListItems = ResizeArray()
        let flushBulletedListItems () =
            let children = List.ofSeq bulletedListItems
            let unorderedList =
                ul [
                    _class "list-disc"
                    _children children
                ]
            elements.Add(unorderedList)
            bulletedListItems.Clear()
        let flushNumberedListItems () =
            let children = List.ofSeq numberedListItems
            let orderedList =
                ol [
                    _class "list-decimal"
                    _children children
                ]
            elements.Add(orderedList)
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
        List.ofSeq elements
    
module Layout =
    let main (extraMeta:HtmlElement list) (page:HtmlElement) =
        html [
            _children [
                head [
                    _children [
                        title "Andrew Meier"
                        meta [ _charset "utf-8" ]
                        meta [
                            _name "viewport"
                            _content "width=device-width, initial-scale=1.0"
                        ]
                        for el in extraMeta do el
                        link [
                            _href "/css/compiled.css"
                            _rel "stylesheet"
                        ]
                        link [
                            _href "/css/prism.css"
                            _rel "stylesheet"
                        ]
                    ]
                ]
                body [
                    _class "bg-gray-50"
                    _children [
                        nav [
                            _class "h-16 bg-gray-100 text-gray-800 border-b-2 border-gray-200"
                            _children [
                                div [
                                    _class "container mx-auto max-w-3xl h-full flex justify-between items-center"
                                    _children [
                                        button [
                                            _class "p-2 rounded text-lg font-medium cursor-pointer hover:bg-gray-200"
                                            _hxGet "/"
                                            _hxTarget "#page"
                                            _children "Andrew's Thoughts"
                                        ]
                                        div [
                                            _class "flex items-center space-x-1"
                                            _children [
                                                button [
                                                    _class "p-2 rounded text-md font-medium cursor-pointer hover:bg-gray-200"
                                                    _hxGet "/about"
                                                    _hxTarget "#page"
                                                    _children "About"
                                                ]
                                                a [
                                                    _class "p-2 rounded-full cursor-pointer hover:bg-gray-200"
                                                    _href "https://twitter.com/ameier38"
                                                    _children Icon.twitter
                                                ]
                                                a [
                                                    _class "p-2 rounded-full cursor-pointer hover:bg-gray-200"
                                                    _href "https://github.com/ameier38"
                                                    _children Icon.github
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        div [
                            _id "page"
                            _children page
                        ]    
                        script [ _src "/scripts/prism.js"; _dataManual ]
                        script [ _src "/scripts/htmx.min.js" ]
                        script [ _src "/scripts/_hyperscript_web.min.js" ]
                    ]
                ]
            ]
        ]
        
module Page =
    
    // ref: https://developer.twitter.com/en/docs/twitter-for-websites/cards/overview/summary
    let postTwitterMetas (post:Post) = [
        meta [
            _name "twitter:card"
            _content "summary"
        ]
        if post.title <> "" then
            meta [
                _name "twitter:title"
                _content post.title
            ]
        if post.summary <> "" then
            meta [
                _name "twitter:description"
                _content post.summary
            ]
        if post.icon <> "" then
            meta [
                _name "twitter:image"
                _content post.icon
            ]
        if post.iconAlt <> "" then
            meta [
                _name "twitter:image:alt"
                _content post.iconAlt
            ]
    ]
        
        
    let private postSummary (post:Post) =
        div [
            _id post.permalink
            _class "relative border-b-2 border-gray-200 p-2 cursor-pointer hover:bg-gray-100"
            // i.e., when we click this div, make a GET request to /<post-id>
            _hxGet $"/{post.id}"
            // i.e., take the response from the above GET request and replace the element with id 'page'
            _hxTarget "#page"
            _children [
                div [
                    _class "flex justify-between"
                    _children [
                        h3 [
                            _class "text-lg font-medium text-gray-800 mb-2"
                            _children post.title
                        ]
                        p [
                            _class "text-sm text-gray-500 leading-7"
                            _children (post.createdAt.ToString("MM/dd/yyyy"))
                        ]
                    ]
                ]
                p [
                    _class "text-sm text-gray-500"
                    _children post.summary
                ]
                div [
                    _class "loader absolute inset-0 w-full h-full bg-gray-500/25"
                    _children [
                        div [
                            _class "flex justify-center items-center h-full"
                            _children Spinner.circle
                        ]
                    ]
                ]
            ]
        ]
        
    let postList (posts:Post[]) =
        div [
            _class "container mx-auto max-w-3xl"
            _children [
                for post in posts do
                    postSummary post
            ]
        ]
        
    let postDetail (detail:PostDetail) =
        div [
            _class "w-full"
            _children [
                div [
                    _class "bg-no-repeat bg-center bg-cover bg-blend-overlay bg-gray-800 mb-2"
                    _style $"background-image: url('{detail.post.cover}')"
                    _children [
                        div [
                            _class "container mx-auto max-w-3xl p-2"
                            _children [
                                h1 [
                                    _id "title"
                                    _class "text-3xl text-gray-200 font-bold mb-14"
                                    _children detail.post.title
                                ]
                                p [
                                    let createdAt = detail.post.createdAt.ToString("M/d/yyyy")
                                    _class "text-gray-400"
                                    _children $"Created: {createdAt}"
                                ]
                                p [
                                    let updatedAt = detail.post.updatedAt.ToString("M/d/yyyy")
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
                            _hyper "on load call Prism.highlightAllUnder(me)"
                            _children (Content.toHtml detail.content)
                        ]
                    ]
                ]
            ]
        ]
        
    let notFound =
        div [
            _class "flex justify-center"
            _children [
                div [
                    _class "flex flex-col items-center pt-4"
                    _children [
                        h1 [
                            _class "text-3xl font-medium text-gray-800 mb-4"
                            _children "Page not found"
                        ]
                        button [
                            _class "rounded p-2 bg-gray-50 text-gray-800 border-gray-800 border hover:bg-gray-100 cursor-pointer"
                            _hxGet "/"
                            _hxTarget "#page"
                            _children "Back to home"
                        ]
                    ]
                ]
            ]
        ]
    

// Specifies that this controller handles the index route (i.e., https://andrewmeier.dev/)
[<Route("")>]
type PostController(client:IPostClient) =
    inherit Controller()
    
    member private this.Render(page:HtmlElement, ?extraMetas:HtmlElement list) =
        let extraMetas = extraMetas |> Option.defaultValue List.empty
        let html =
            if this.IsHtmx then Render.view page
            else page |> Layout.main extraMetas |> Render.document
        this.Html(html)
    
    // No route specified so use the controller route
    [<HttpGet>]
    member this.Index() = task {
        let! posts = client.List()
        let page =
            posts
            |> Array.filter (fun p -> p.permalink <> "about")
            |> Page.postList
        if this.IsHtmx then this.HxPush("/")
        return this.Render(page)
    }
        
    [<Route("404")>]
    [<HttpGet>]
    member this.NotFound() =
        let page = Page.notFound
        if this.IsHtmx then this.HxPush("/404")
        this.Render(page)
        
    // Routes matching post ids, e.g., /<guid>
    [<Route("{postId:guid}")>]
    [<HttpGet>]
    member this.PostById(postId:string) = task {
        match! client.GetById(postId) with
        | Some postDetail ->
            let metas = Page.postTwitterMetas postDetail.post
            let page = Page.postDetail postDetail
            if this.IsHtmx then this.HxPush($"/{postDetail.post.permalink}")
            return this.Render(page, metas)
        | None ->
            let page = Page.notFound
            if this.IsHtmx then this.HxPush("/404")
            return this.Render(page)
    }
    
    // Routes matching permalinks, e.g., /blogging-with-fsharp
    [<Route("{permalink:regex(^[[a-z-]]+$)}")>]
    [<HttpGet>]
    member this.PostByPermalink(permalink:string) = task {
        match! client.GetByPermalink(permalink) with
        | Some postDetail ->
            let metas = Page.postTwitterMetas postDetail.post
            let page = Page.postDetail postDetail
            if this.IsHtmx then this.HxPush($"/{permalink}")
            return this.Render(page, metas)
        | None ->
            let page = Page.notFound
            if this.IsHtmx then this.HxPush("/404")
            return this.Render(page)
    }
