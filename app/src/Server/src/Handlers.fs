module Server.Handlers

open Giraffe
open FSharp.ViewEngine
open Server.NotionClient
open Server.Components
open Server.Views

type Response =
    { page:Element
      meta:Element seq
      push:string option }
    
module Response =
    let create (page:Element) =
        { page = page
          meta = Seq.empty
          push = None }
    let withMeta (meta:Element seq) (response:Response) =
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
                |> Element.render
                |> ctx.WriteHtmlStringAsync
            | _ ->
                Layout.main response.meta response.page
                |> Element.render
                |> ctx.WriteHtmlStringAsync)
    
let getProjects : HttpHandler =
    projectsPage
    |> Response.create
    |> Response.withPush "/projects"
    |> Response.render
    
let private getPosts: HttpHandler =
    fun next ctx -> task {
        let client = ctx.GetService<INotionClient>()
        let! posts = client.List()
        let response =
            posts
            |> Array.filter (fun p -> p.permalink <> "index")
            |> postsPage
            |> Response.create
            |> Response.withPush "/"
        return! Response.render response next ctx
    }
    
let private getPostByPermalink (permalink:string) : HttpHandler =
    fun next ctx -> task {
        let client = ctx.GetService<INotionClient>()
        match! client.GetByPermalink(permalink) with
        | Some detail ->
            let metas = TwitterMeta.fromPageProperties detail.properties
            let response =
                postPage detail
                |> Response.create
                |> Response.withMeta metas
                |> Response.withPush $"/{permalink}"
            return! Response.render response next ctx
        | None ->
            let response = Response.create Page.notFound
            return! Response.render response next ctx
    }
    
let getAbout:HttpHandler =
    fun next ctx -> task {
        let notion = ctx.GetService<INotionClient>()
        match! notion.GetByPermalink("index") with
        | Some p ->
            let view = aboutPage p
            let meta = TwitterMeta.fromPageProperties p.properties
            let res =
                Response.create view
                |> Response.withMeta meta
                |> Response.withPush "/about"
            return! Response.render res next ctx
        | None ->
            let response = Response.create Page.notFound
            return! Response.render response next ctx
    }
    
let notFound : HttpHandler =
    Page.notFound
    |> Response.create
    |> Response.render
    
let app:HttpHandler =
    choose [
        routex "(/?)" >=> GET >=> getPosts
        route "/about" >=> GET >=> getAbout
        route "/projects" >=> GET >=> getProjects
        routef "/%s" getPostByPermalink
        notFound
    ]
