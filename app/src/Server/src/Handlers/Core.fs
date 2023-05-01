module Server.Handlers.Core

open Giraffe
open Server.ViewEngine
open Server.Components

type Response =
    { page:HtmlElement
      meta:HtmlElement seq
      push:string option }
    
module Response =
    let create (page:HtmlElement) =
        { page = page
          meta = Seq.empty
          push = None }
    let withMeta (meta:HtmlElement seq) (response:Response) =
        { response with meta = meta }
    let withPush (push:string) (response:Response) =
        { response with push = Some push }
    
let render (response:Response) : HttpHandler =
    handleContext(fun ctx ->
        match ctx.TryGetRequestHeader "HX-Request" with
        | Some "true" ->
            match response.push with
            | Some push -> ctx.SetHttpHeader("HX-Push", push)
            | _ -> ()
            if Seq.isEmpty response.meta then response.page
            else Html.fragment [ yield! response.meta; response.page ]
            |> Render.view
            |> ctx.WriteHtmlStringAsync
        | _ ->
            Layout.main response.meta response.page
            |> Render.document
            |> ctx.WriteHtmlStringAsync)
