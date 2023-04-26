module Server.Handlers.Post

// open Giraffe
// open Server.Config
// open Server.PostClient
// open Server.Components
//
// let private index : HttpHandler =
//     fun next ctx -> task {
//         let notionConfig = ctx.GetService<NotionConfig>()
//         let client = ctx.GetService<IPostClient>()
//         let! posts = client.List(notionConfig.BlogDatabaseId)
//         let page =
//             posts
//             |> Array.filter (fun p -> p.permalink <> "about")
//             |> Page.postList
//         return! Core.render page [] "/" next ctx
//     }
//     
// let private articleByIdHandler (pageId:System.Guid) : HttpHandler =
//     fun next ctx -> task {
//         let client = ctx.GetService<IPostClient>()
//         let pageId = pageId.ToString("N")
//         match! client.GetById(pageId) with
//         | Some postDetail ->
//             let metas = Page.postTwitterMetas postDetail.post
//             let page = Page.postDetail postDetail
//             let path = $"/{postDetail.post.permalink}"
//             return! Core.render page metas path next ctx
//         | None ->
//             return! Core.render Page.notFound [] "/404" next ctx
//     }
//     
// let private postByPermalinkHandler (permalink:string) : HttpHandler =
//     fun next ctx -> task {
//         let client = ctx.GetService<IPostClient>()
//         match! client.GetByPermalink(permalink) with
//         | Some postDetail ->
//             let metas = Page.postTwitterMetas postDetail.post
//             let page = Page.postDetail postDetail
//             let path = $"/{permalink}"
//             return! render page metas path next ctx
//         | None ->
//             return! render Page.notFound [] "/404" next ctx
//     }
//     
// let private notFoundHandler: HttpHandler =
//     render Page.notFound [] "/404"
//     
// let postApp : HttpHandler =
//     choose [
//         GET >=> choose [
//             route "/" >=> indexHandler
//             routef "/%O" postByIdHandler
//             routef "/%s" postByPermalinkHandler
//         ]
//         notFoundHandler
//     ]
