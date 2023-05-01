module Server.Handlers.Articles

open Giraffe
open Server.NotionClient
open Server.Components

module View = Server.Views.Articles

let private getArticles: HttpHandler =
    fun next ctx -> task {
        let client = ctx.GetService<INotionClient>()
        let! articles = client.List()
        let response =
            articles
            |> Array.filter (fun p -> p.permalink <> "index")
            |> View.articlesPage
            |> Core.Response.create
            |> Core.Response.withPush "/articles"
        return! Core.render response next ctx
    }
    
let private getArticleByPermalink (permalink:string) : HttpHandler =
    fun next ctx -> task {
        let client = ctx.GetService<INotionClient>()
        match! client.GetByPermalink(permalink) with
        | Some detail ->
            let metas = TwitterMeta.fromPageProperties detail.properties
            let response =
                View.articlePage detail
                |> Core.Response.create
                |> Core.Response.withMeta metas
                |> Core.Response.withPush $"/articles/{permalink}"
            return! Core.render response next ctx
        | None ->
            let response = Core.Response.create View.notFoundPage
            return! Core.render response next ctx
    }
    
let private notFound: HttpHandler =
    View.notFoundPage
    |> Core.Response.create 
    |> Core.render
    
let app : HttpHandler =
    choose [
        GET >=> choose [
            routex "(/?)" >=> getArticles
            routef "/%s" getArticleByPermalink
        ]
        notFound
    ]
