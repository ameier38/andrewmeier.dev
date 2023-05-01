module Server.Handlers.Index

open Server.NotionClient
open Server.Components
open Serilog
open Giraffe

module View = Server.Views.Index

let getIndex:HttpHandler =
    fun next ctx -> task {
        let notion = ctx.GetService<INotionClient>()
        match! notion.GetByPermalink("index") with
        | Some p ->
            let view = View.indexPage p
            let meta = TwitterMeta.fromPageProperties p.properties
            let res =
                Core.Response.create view
                |> Core.Response.withMeta meta
                |> Core.Response.withPush "/"
            return! Core.render res next ctx
        | None ->
            let view = View.notFoundPage
            let res =
                Core.Response.create view
                |> Core.Response.withPush "/"
            return! Core.render res next ctx
    }
    
let getProjects : HttpHandler =
    Server.Views.Projects.projectsPage
    |> Core.Response.create
    |> Core.Response.withPush "/projects"
    |> Core.render
    
let notFound : HttpHandler =
    View.notFoundPage
    |> Core.Response.create
    |> Core.render
    
let app:HttpHandler =
    choose [
        routex "(/?)" >=> GET >=> getIndex
        route "/projects" >=> GET >=> getProjects
        subRoute "/articles" Articles.app
        // previous version hosted articles at index
        routexp "/([-a-z]+)" (fun groups -> redirectTo true $"/articles/{Seq.item 1 groups}")
        notFound
    ]
