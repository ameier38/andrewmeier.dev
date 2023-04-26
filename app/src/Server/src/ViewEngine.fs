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
        | Children of HtmlElement seq   // e.g., div [ p "Hello" ] -> <div><p>Hello</p></div>
        | EmptyAttribute                // No op
        
    and HtmlElement =
        | Element of string * HtmlAttribute seq         // e.g., <h1>Hello</h1>
        | VoidElement of string * HtmlAttribute seq     // e.g., <br/>
        | TextElement of string                         // Text content
        | Fragment of HtmlElement seq
        | EmptyElement                                  // No op


// ---------------------------
// Internal ViewBuilder
// ---------------------------

module private ViewBuilder =
    open System.Text
    
    let inline (+=) (sb:StringBuilder) (s:string) = sb.Append(s)
    let inline (+!) (sb:StringBuilder) (s:string) = sb.Append(s) |> ignore
    
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
        | TextElement text -> sb +! text
        | Fragment children -> for child in children do buildElement child sb
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
    static member fragment (children:HtmlElement seq) = Fragment children
    static member raw (v:string) = TextElement v
    static member html (attrs:HtmlAttribute list) = Element ("html", attrs)
    static member head (attrs:HtmlAttribute list) = Element ("head", attrs)
    static member title (value:string) = Element ("title", [ Children [ TextElement value ] ])
    static member meta (attrs:HtmlAttribute seq) = VoidElement ("meta", attrs)
    static member link (attrs:HtmlAttribute seq) = VoidElement ("link", attrs)
    static member script (attrs:HtmlAttribute seq) = Element ("script", attrs)
    static member body (attrs:HtmlAttribute seq) = Element ("body", attrs)
    static member header (attrs:HtmlAttribute seq) = Element ("header", attrs)
    static member nav (attrs:HtmlAttribute seq) = Element ("nav", attrs)
    static member h1 (attrs:HtmlAttribute seq) = Element ("h1", attrs)
    static member h2 (attrs:HtmlAttribute seq) = Element ("h2", attrs)
    static member h3 (attrs:HtmlAttribute seq) = Element ("h3", attrs)
    static member div (attrs:HtmlAttribute seq) = Element ("div", attrs)
    static member p (attrs:HtmlAttribute seq) = Element ("p", attrs)
    static member p (text:string) = Element ("p", [ Children [ TextElement text ] ])
    static member span (attrs:HtmlAttribute seq) = Element ("span", attrs)
    static member span (text:string) = Element ("span", [ Children [ TextElement text ] ])
    static member a (attrs:HtmlAttribute seq) = Element ("a", attrs)
    static member button (attrs:HtmlAttribute seq) = Element ("button", attrs)
    static member img (attrs:HtmlAttribute seq) = Element ("img", attrs)
    static member code (attrs:HtmlAttribute seq) = Element ("code", attrs)
    static member pre (attrs:HtmlAttribute seq) = Element ("pre", attrs)
    static member br = VoidElement("br", [])
    static member ul (attrs:HtmlAttribute seq) = Element ("ul", attrs)
    static member ol (attrs:HtmlAttribute seq) = Element ("ol", attrs)
    static member li (attrs:HtmlAttribute seq) = Element ("li", attrs)
    static member blockquote (attrs:HtmlAttribute seq) = Element ("blockquote", attrs)
    
    static member _id (v:string) = KeyValue ("id", v)
    static member _class (v:string) = KeyValue ("class", v)
    static member _class (v:string seq) = KeyValue ("class", v |> String.concat " ")
    static member _style (v:string) = KeyValue ("style", v)
    static member _children (v:HtmlElement seq) = Children v
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
    static member svg (attrs:HtmlAttribute seq) = Element ("svg", attrs)
    static member path (attrs:HtmlAttribute seq) = Element ("path", attrs)
    static member circle (attrs:HtmlAttribute seq) = Element ("circle", attrs)
    
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
    static member _hxSwap (v:string) = KeyValue ("hx-swap", v)
    static member _hxSwapOOB (v:string) = KeyValue ("hx-swap-oob", v)
    
type Alpine() =
    static member _xOn (event:string, v:string) = KeyValue ($"x-on:{event}", v)
    static member _xOn (event:string) = Boolean $"x-on:{event}"
    static member _xInit (v:string) = KeyValue ("x-init", v)
    static member _xData (v:string) = KeyValue ("x-data", v)
    static member _xRef (v:string) = KeyValue ("x-ref", v)
    static member _xText (v:string) = KeyValue ("x-text", v)
    static member _xBind (attr:string, v:string) = KeyValue ($"x-bind:{attr}", v)
    static member _xShow (v:string) = KeyValue ("x-show", v)
    static member _xIf (v:string) = KeyValue ("x-if", v)
    static member _xFor (v:string) = KeyValue ("x-for", v)
    static member _xModel (v:string) = KeyValue ("x-model", v)
    static member _xId (v:string) = KeyValue ("x-id", v)
    static member _xEffect (v:string) = KeyValue ("x-effect", v)
    static member _xTransition (?modifier:string) =
        match modifier with
        | Some m -> Boolean $"x-transition.{m}"
        | None -> Boolean "x-transition"
    static member _xTrap (v:string, ?modifier:string) =
        match modifier with
        | Some m -> KeyValue ($"x-trap.{m}", v)
        | None -> KeyValue ("x-trap", v)
