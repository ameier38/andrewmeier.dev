module Server.HttpHandlers

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Giraffe
open Microsoft.AspNetCore.Http

let routeBuilder (typeName:string) (methodName:string) = $"/api/{typeName}/{methodName}"

let createPostApiFromContext (httpContext:HttpContext) =
    let postClient = httpContext.GetService<PostClient.IPostClient>()
    Api.postApi postClient

let api: HttpHandler =
    Remoting.createApi()
    |> Remoting.fromContext createPostApiFromContext
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.buildHttpHandler

let app: HttpHandler = choose [
    GET >=> route "/healthz" >=> Successful.OK "Healthy!"
    api
]
