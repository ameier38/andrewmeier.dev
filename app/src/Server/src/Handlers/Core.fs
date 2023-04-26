module Server.Handlers.Core

open Giraffe
open Server.ViewEngine
open Server.Types
open Server.Components

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
