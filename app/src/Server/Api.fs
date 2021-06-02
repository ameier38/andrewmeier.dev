module Server.Api

open Shared.PostStore
open Fable.Remoting.Server
open Fable.Remoting.AspNetCore
open Microsoft.AspNetCore.Http

let routeBuilder (typeName:string) (methodName:string) = $"/api/{typeName}/{methodName}"

let createPostStoreFromContext (httpContext:HttpContext) =
    let postClient = httpContext.GetService<PostClient.IPostClient>()
    PostStore.postStore postClient

let postApi: RemotingOptions<HttpContext,IPostStore> =
    Remoting.createApi()
    |> Remoting.fromContext createPostStoreFromContext
    |> Remoting.withRouteBuilder routeBuilder
