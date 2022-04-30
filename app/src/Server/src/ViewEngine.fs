module Server.ViewEngine

// ---------------------------
// Default HTML elements
// ---------------------------

[<AutoOpen>]
module HtmlElements =

    // ---------------------------
    // Definition of different HTML content
    //
    // For more info check:
    // - https://developer.mozilla.org/en-US/docs/Web/HTML/Element
    // - https://www.w3.org/TR/html5/syntax.html#void-elements
    // ---------------------------

    type HtmlAttribute =
        | KeyValue of string * string   // e.g., div [ _class "text-xl" ] -> <div class="text-xl"></div>
        | Boolean of string             // e.g., button [ _disabled ] -> <button disabled></button>
        | Children of HtmlElement list  // e.g., div [ p "Hello" ] -> <div><p>Hello</p></div>
        | EmptyAttribute                // No op
        
    and HtmlElement =
        | Element of string * HtmlAttribute list        // e.g., <h1>Hello</h1>
        | VoidElement of string * HtmlAttribute list    // e.g., <br/>
        | TextElement of string                         // Text content
        | EmptyElement                                  // No op


// ---------------------------
// Internal ViewBuilder
// ---------------------------

module private ViewBuilder =
    open System.Text
    
    let inline (+=) (sb:StringBuilder) (s:string) = sb.Append(s)
    let inline (+!) (sb:StringBuilder) (s:string) = sb.Append(s) |> ignore
    
    let encode v = System.Net.WebUtility.HtmlEncode v
    
    let rec buildElement (el:HtmlElement) (sb:StringBuilder) =
        match el with
        | Element (tag, attributes) ->
            sb += "<" +! tag
            let children = ResizeArray()
            for attr in attributes do
                match attr with
                | KeyValue (key, value) -> sb += " " += key += "=\"" += value +! "\""
                | Boolean key -> sb += " " +! key
                | Children elements -> children.AddRange(elements)
                | EmptyAttribute -> ()
            sb +! ">"
            for child in children do buildElement child sb
            sb += "</" += tag +! ">"
        | VoidElement (tag, attributes) ->
            sb += "<" +! tag
            for attr in attributes do
                match attr with
                | KeyValue (key, value) -> sb += " " += key += "=\"" += value +! "\""
                | Boolean key -> sb += " " +! key
                | Children _ -> failwith "void elements cannot have children"
                | EmptyAttribute -> ()
            sb +! ">"
        | TextElement text -> sb +! (encode text)
        | EmptyElement -> ()

// ---------------------------
// Render HTML/XML views
// ---------------------------

[<RequireQualifiedAccess>]
module Render =
    open System.Text
    open ViewBuilder
    
    let view (html:HtmlElement) =
        let sb = StringBuilder()
        buildElement html sb
        sb.ToString()
        
    let document (html:HtmlElement) =
        let sb = StringBuilder()
        sb += "<!DOCTYPE html>" +! System.Environment.NewLine
        buildElement html sb
        sb.ToString()
        
type Html() =
    static member empty = EmptyElement
    static member raw (v:string) = TextElement v
    static member html (attrs:HtmlAttribute list) = Element ("html", attrs)
    static member head (attrs:HtmlAttribute list) = Element ("head", attrs)
    static member title (value:string) = Element ("title", [ Children [ TextElement value ] ])
    static member meta (attrs:HtmlAttribute list) = VoidElement ("meta", attrs)
    static member link (attrs:HtmlAttribute list) = VoidElement ("link", attrs)
    static member script (attrs:HtmlAttribute list) = Element ("script", attrs)
    static member body (attrs:HtmlAttribute list) = Element ("body", attrs)
    static member nav (attrs:HtmlAttribute list) = Element ("nav", attrs)
    static member h1 (attrs:HtmlAttribute list) = Element ("h1", attrs)
    static member h2 (attrs:HtmlAttribute list) = Element ("h2", attrs)
    static member h3 (attrs:HtmlAttribute list) = Element ("h3", attrs)
    static member div (attrs:HtmlAttribute list) = Element ("div", attrs)
    static member p (attrs:HtmlAttribute list) = Element ("p", attrs)
    static member p (text:string) = Element ("p", [ Children [ TextElement text ] ])
    static member span (attrs:HtmlAttribute list) = Element ("span", attrs)
    static member span (text:string) = Element ("span", [ Children [ TextElement text ] ])
    static member a (attrs:HtmlAttribute list) = Element ("a", attrs)
    static member button (attrs:HtmlAttribute list) = Element ("button", attrs)
    static member img (attrs:HtmlAttribute list) = Element ("img", attrs)
    static member code (attrs:HtmlAttribute list) = Element ("code", attrs)
    static member pre (attrs:HtmlAttribute list) = Element ("pre", attrs)
    static member br = VoidElement("br", [])
    static member ul (attrs:HtmlAttribute list) = Element ("ul", attrs)
    static member ol (attrs:HtmlAttribute list) = Element ("ol", attrs)
    static member li (attrs:HtmlAttribute list) = Element ("li", attrs)
    static member blockquote (attrs:HtmlAttribute list) = Element ("blockquote", attrs)
    
    static member _id (v:string) = KeyValue ("id", v)
    static member _class (v:string) = KeyValue ("class", v)
    static member _class (v:string list) = KeyValue ("class", v |> String.concat " ")
    static member _style (v:string) = KeyValue ("style", v)
    static member _children (v:HtmlElement list) = Children v
    static member _children (v:HtmlElement) = Children [ v ]
    static member _children (v:string) = Children [ TextElement v ]
    static member _lang (v:string) = KeyValue ("lang", v)
    static member _charset (v:string) = KeyValue ("charset", v)
    static member _name (v:string) = KeyValue ("name", v)
    static member _content (v:string) = KeyValue ("content", v)
    static member _href (v:string) = KeyValue ("href", v)
    static member _rel (v:string) = KeyValue ("rel", v)
    static member _src (v:string) = KeyValue ("src", v)
    static member _action (v:string) = KeyValue("action", v)
    static member _method (v:string) = KeyValue("method", v)
    static member _dataManual = Boolean("data-manual")
    
type Svg() =
    static member svg (attrs:HtmlAttribute list) = Element ("svg", attrs)
    static member path (attrs:HtmlAttribute list) = Element ("path", attrs)
    static member circle (attrs:HtmlAttribute list) = Element ("circle", attrs)
    
    static member _viewBox (v:string) = KeyValue ("viewBox", v)
    static member _width (v:int) = KeyValue ("width", string v)
    static member _height (v:int) = KeyValue ("height", string v)
    static member _fill (v:string) = KeyValue ("fill", v)
    static member _stroke (v:string) = KeyValue ("stroke", v)
    static member _strokeWidth (v:int) = KeyValue ("stroke-width", string v)
    static member _strokeLinecap (v:string) = KeyValue ("stroke-linecap", v)
    static member _strokeLinejoin (v:string) = KeyValue ("stroke-linejoin", v)
    static member _fillRule (v:string) = KeyValue ("fill-rule", v)
    static member _clipRule (v:string) = KeyValue ("clip-rule", v)
    static member _d (v:string) = KeyValue ("d", v)
    static member _cx (v:int) = KeyValue ("cx", string v)
    static member _cy (v:int) = KeyValue ("cy", string v)
    static member _r (v:int) = KeyValue ("r", string v)
    
type Htmx() =
    static member _hxGet (v:string) = KeyValue ("hx-get", v)
    static member _hxGet (v:string option) = match v with Some v -> KeyValue ("hx-get", v) | None -> EmptyAttribute
    static member _hxPost (v:string) = KeyValue("hx-post", v)
    static member _hxPost (v:string option) = match v with Some v -> KeyValue("hx-post", v) | None -> EmptyAttribute
    static member _hxTrigger (v:string) = KeyValue ("hx-trigger", v)
    static member _hxTarget (v:string) = KeyValue ("hx-target", v)
    static member _hxTarget (v:string option) = match v with Some v -> KeyValue ("hx-target", v) | None -> EmptyAttribute
    
type Hyper() =
    static member _hyper (v:string) = KeyValue ("_", v)
