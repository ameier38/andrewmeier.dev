module Server.Handlers.Index

open Server.NotionClient
open Server.Types
open Server.Components
open Giraffe

module View = Server.Views.Index

let getIndexPage:HttpHandler =
    fun next ctx -> task {
        let notion = ctx.GetService<INotionClient>()
        match! notion.GetByPermalink("home") with
        | Some p ->
            let view = View.indexPage p
            let meta = TwitterMeta.fromPageProperties p.properties
            let res = Response.create view |> Response.withMeta meta |> Response.withPush "/"
            return! Core.render res next ctx
        | None ->
            let view = View.notFoundPage
            let res = Response.create view |> Response.withPush "/"
            return! Core.render res next ctx
    }
    
let app:HttpHandler =
    choose [
        GET >=> choose [
            routex "(/?)" >=> getIndexPage
        ]
    ]
