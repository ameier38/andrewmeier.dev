module Server.Components

open Notion.Client
open Server.NotionClient
open Server.ViewEngine
open Serilog

open type Html
open type Svg
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
        | :? HeadingThreeBlock as b ->
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
        
module Layout =
    let private navigationItem (text:string) (href:string) =
        a [
            _xBind ("class", $"selectedNav === '{text}' && 'text-emerald-600'")
            _class "relative block px-3 py-2 hover:text-emerald-600 hover:cursor-pointer"
            _hxGet href
            _hxTarget "#page"
            _children text
        ]
        
    let navigation =
        nav [
            _children [
                ul [
                    _class "flex rounded-full bg-white/90 ring-1 ring-gray-900/5 shadow-lg shadow-gray-800/5 px-3 text-sm font-medium text-gray-800"
                    _children [
                        navigationItem "About" "/"
                        navigationItem "Articles" "/articles"
                        navigationItem "Projects" "/projects"
                    ]
                ]
            ]
        ]
        
    let main (extraMeta:HtmlElement seq) (page:HtmlElement) =
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
                    _xData "{ selectedNav: '' }"
                    _class "bg-gray-100"
                    _children [
                        div [
                            _class "fixed inset-0 flex justify-center sm:px-8"
                            _children [
                                div [
                                    _class "w-full max-w-7xl bg-gray-50 ring-1 ring-gray-200"
                                ]
                            ]
                        ]
                        div [
                            _class "relative"
                            _children [
                                header [
                                    _class "h-16 pt-6 mx-auto flex justify-center"
                                    _children navigation
                                ]
                                div [
                                    _id "page"
                                    _class "mx-auto max-w-7xl mt-16 sm:px-8"
                                    _children [
                                        div [
                                            _class "px-4"
                                            _children page
                                        ]
                                    ]
                                ]    
                            ]
                        ]
                        script [ _src "/scripts/prism.js"; _dataManual ]
                        script [ _src "/scripts/htmx.1.8.6.min.js" ]
                        script [ _src "/scripts/alpinejs.3.12.0.min.js" ]
                    ]
                ]
            ]
        ]
