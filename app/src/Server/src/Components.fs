module Server.Components

open FSharp.ViewEngine
open Notion.Client
open Server.NotionClient
open Serilog

open type Html
open type Html
open type Htmx
open type Alpine

module Icon =
    let github =
        raw """
        <svg viewBox="0 0 24 24" aria-hidden="true" class="h-6 w-6" fill="currentColor">
            <path fill-rule="evenodd" clip-rule="evenodd" d="M12 2C6.477 2 2 6.463 2 11.97c0 4.404 2.865 8.14 6.839 9.458.5.092.682-.216.682-.48 0-.236-.008-.864-.013-1.695-2.782.602-3.369-1.337-3.369-1.337-.454-1.151-1.11-1.458-1.11-1.458-.908-.618.069-.606.069-.606 1.003.07 1.531 1.027 1.531 1.027.892 1.524 2.341 1.084 2.91.828.092-.643.35-1.083.636-1.332-2.22-.251-4.555-1.107-4.555-4.927 0-1.088.39-1.979 1.029-2.675-.103-.252-.446-1.266.098-2.638 0 0 .84-.268 2.75 1.022A9.607 9.607 0 0 1 12 6.82c.85.004 1.705.114 2.504.336 1.909-1.29 2.747-1.022 2.747-1.022.546 1.372.202 2.386.1 2.638.64.696 1.028 1.587 1.028 2.675 0 3.83-2.339 4.673-4.566 4.92.359.307.678.915.678 1.846 0 1.332-.012 2.407-.012 2.734 0 .267.18.577.688.48 3.97-1.32 6.833-5.054 6.833-9.458C22 6.463 17.522 2 12 2Z"></path>
        </svg>
        """
        
    let twitter =
        raw """
        <svg viewBox="0 0 20 20" aria-hidden="true" class="h-5 w-5" fill="currentColor">
            <path d="M6.29 18.251c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0 0 20 3.92a8.19 8.19 0 0 1-2.357.646 4.118 4.118 0 0 0 1.804-2.27 8.224 8.224 0 0 1-2.605.996 4.107 4.107 0 0 0-6.993 3.743 11.65 11.65 0 0 1-8.457-4.287 4.106 4.106 0 0 0 1.27 5.477A4.073 4.073 0 0 1 .8 7.713v.052a4.105 4.105 0 0 0 3.292 4.022 4.095 4.095 0 0 1-1.853.07 4.108 4.108 0 0 0 3.834 2.85A8.233 8.233 0 0 1 0 16.407a11.615 11.615 0 0 0 6.29 1.84"></path>
        </svg>
        """
        
    let linkedIn =
        raw """
        <svg viewBox="0 0 24 24" aria-hidden="true" class="h-6 w-6" fill="currentColor">
            <path d="M18.335 18.339H15.67v-4.177c0-.996-.02-2.278-1.39-2.278-1.389 0-1.601 1.084-1.601 2.205v4.25h-2.666V9.75h2.56v1.17h.035c.358-.674 1.228-1.387 2.528-1.387 2.7 0 3.2 1.778 3.2 4.091v4.715zM7.003 8.575a1.546 1.546 0 01-1.548-1.549 1.548 1.548 0 111.547 1.549zm1.336 9.764H5.666V9.75H8.34v8.589zM19.67 3H4.329C3.593 3 3 3.58 3 4.297v15.406C3 20.42 3.594 21 4.328 21h15.338C20.4 21 21 20.42 21 19.703V4.297C21 3.58 20.4 3 19.666 3h.003z"></path>
        </svg>
        """
        
    let chevronRight =
        raw """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="w-5 h-5">
          <path fill-rule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clip-rule="evenodd" />
        </svg>
        """
        
    let link =
        raw """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="w-5 h-5">
          <path d="M12.232 4.232a2.5 2.5 0 013.536 3.536l-1.225 1.224a.75.75 0 001.061 1.06l1.224-1.224a4 4 0 00-5.656-5.656l-3 3a4 4 0 00.225 5.865.75.75 0 00.977-1.138 2.5 2.5 0 01-.142-3.667l3-3z" />
          <path d="M11.603 7.963a.75.75 0 00-.977 1.138 2.5 2.5 0 01.142 3.667l-3 3a2.5 2.5 0 01-3.536-3.536l1.225-1.224a.75.75 0 00-1.061-1.06l-1.224 1.224a4 4 0 105.656 5.656l3-3a4 4 0 00-.225-5.865z" />
        </svg>
        """
        
type Tag =
    static member primary(text:string, ?attrs:Attribute seq) =
        let attrs = defaultArg attrs Seq.empty
        span [
            _class "inline-flex items-center rounded-md bg-gray-50 px-2 py-1 text-xs font-medium text-gray-600 ring-1 ring-inset ring-gray-500/10"
            _children text
            yield! attrs
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
            h2 [
                _class "mt-8"
                _id (b.Id.Replace("-", ""))
                _children (b.Heading_1.RichText |> Seq.map RichTextBase.toHtml |> Seq.toList)
            ]
        | :? HeadingTwoBlock as b ->
            h3 [
                _class "mt-6"
                _id (b.Id.Replace("-", ""))
                _children (b.Heading_2.RichText |> Seq.map RichTextBase.toHtml |> Seq.toList)
            ]
        | :? HeadingThreeBlock as b ->
            h4 [
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
                    for child in b.BulletedListItem.Children do
                        toHtml child
                ]
            ]
        | :? NumberedListItemBlock as b ->
            li [
                _children [
                    for text in b.NumberedListItem.RichText do
                        RichTextBase.toHtml text
                    for child in b.NumberedListItem.Children do
                        toHtml child
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
            Element.Noop
            
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
        if numberedListItems.Count > 0 then flushNumberedListItems()
        if bulletedListItems.Count > 0 then flushBulletedListItems()
        List.ofSeq elements
        
module TwitterMeta =
    let fromPageProperties (props:PageProperties) =
        [
            meta [
                _hxSwapOOB "true"
                _id "twitter-card"
                _name "twitter:card"
                _content "summary"
            ]
            if props.title <> "" then
                meta [
                    _hxSwapOOB "true"
                    _id "twitter-title"
                    _name "twitter:title"
                    _content props.title
                ]
            if props.summary <> "" then
                meta [
                    _hxSwapOOB "true"
                    _name "twitter:description"
                    _id "twitter-description"
                    _content props.summary
                ]
            if props.icon <> "" then
                meta [
                    _hxSwapOOB "true"
                    _id "twitter-image"
                    _name "twitter:image"
                    _content props.icon
                ]
            if props.iconAlt <> "" then
                meta [
                    _hxSwapOOB "true"
                    _id "twitter-image-alt"
                    _name "twitter:image:alt"
                    _content props.iconAlt
                ]
        ]
        
module Page =
    let notFound =
        div [
            _class "flex flex-col items-center"
            _children [
                h1 [
                    _class "text-3xl text-gray-800"
                    _children "Oops! Could not find page."
                ]
                p [
                    _class "mt-2 text-md text-gray-600"
                    _children "Something went wrong. Try refreshing the page."
                ]
            ]
        ]
    
module Layout =
    let private navigationItem (text:string) (href:string) =
        a [
            _xBind ("class", $"selectedNav === '{text}' && 'text-emerald-600'")
            _class "relative block px-3 py-2 hover:text-emerald-600 hover:cursor-pointer"
            _hxGet href
            _hxTarget "#page"
            _hxIndicator "#page-loading"
            _children text
        ]
        
    let topNavigation =
        nav [
            _class "pt-4 mx-auto flex justify-center items-center"
            _children [
                ul [
                    _class "flex rounded-full bg-white/90 ring-1 ring-gray-900/5 shadow-lg shadow-gray-800/5 px-3 text-sm font-medium text-gray-800"
                    _children [
                        navigationItem "Posts" "/"
                        navigationItem "Projects" "/projects"
                        navigationItem "About" "/about"
                    ]
                ]
            ]
        ]
        
    let bottomNavigation =
        div [
            _class "border-t border-gray-200 py-8"
            _children [
                div [
                    _class "flex flex-col sm:flex-row justify-between items-center"
                    _children [
                        div [
                            _class "flex gap-4 text-sm font-medium text-gray-800"
                            _children [
                                navigationItem "Posts" "/"
                                navigationItem "Projects" "/projects"
                                navigationItem "About" "/about"
                            ]
                        ]
                        p [
                            _class "text-sm text-gray-500"
                            _children $"© {System.DateTime.Today.Year} Andrew Meier. All rights reserved."
                        ]
                    ]
                ]
            ]
        ]
        
    let main (extraMeta:Element seq) (page:Element) =
        html [
            _children [
                head [
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
                body [
                    _xData "{ selectedNav: '' }"
                    _class "bg-gray-100"
                    _children [
                        div [
                            _class "fixed top-0 left-0 right-0 z-10 max-w-5xl mx-auto"
                            _children [
                                div [
                                    _id "page-loading"
                                    _class "htmx-loader h-2 bg-emerald-300 animate-pulse"
                                ]
                                topNavigation
                            ]
                        ]
                        div [
                            _class "max-w-5xl mx-auto bg-gray-50 ring-1 ring-gray-200"
                            _children [
                                main [
                                    _id "page"
                                    _class "min-h-screen"
                                    _children page
                                ]
                                footer [
                                    _class "mt-8 px-4 sm:px-8 lg:px-12"
                                    _children bottomNavigation
                                ]
                            ]
                        ]
                        script [ _src "/scripts/prism.1.29.0.js"; _data "manual" ]
                        script [ _src "/scripts/htmx.1.8.6.min.js" ]
                        script [ _src "/scripts/alpinejs.3.12.0.min.js" ]
                    ]
                ]
            ]
        ]
