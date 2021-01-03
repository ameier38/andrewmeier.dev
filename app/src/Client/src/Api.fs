module Client.Api

open Fable.Remoting.Client
open Shared.Api

let routeBuilder (typeName:string) (methodName:string) = $"/api/%s{typeName}/%s{methodName}"

let postApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.buildProxy<IPostApi>
