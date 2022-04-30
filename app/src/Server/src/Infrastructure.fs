namespace global

open System
open System.IO

module Env =

    let variable (key:string) (defaultValue:string) =
        match Environment.GetEnvironmentVariable(key) with
        | s when String.IsNullOrEmpty(s) -> defaultValue
        | s -> s

    let secret secretName secretKey defaultEnv defaultValue =
        let secretsDir = variable "SECRETS_DIR" "/var/secrets" 
        let secretPath = Path.Combine(secretsDir, secretName, secretKey)
        if File.Exists(secretPath) then
            File.ReadAllText(secretPath).Trim()
        else
            variable defaultEnv defaultValue
            
[<AutoOpen>]
module ControllerExtensions =
    open Microsoft.AspNetCore.Mvc
    open Microsoft.Extensions.Primitives
    open System.Net.Mime
    
    type Controller with
        member this.TryGetRequestHeader(key:string) =
            match this.Request.Headers.TryGetValue(key) with
            | true, value -> Some value
            | false, _ -> None
            
        member this.SetResponseHeader(key:string, value:string) =
            this.Response.Headers.Add(key, StringValues value)
            
        member this.IsHtmx =
            match this.TryGetRequestHeader("HX-Request") with
            | Some _ -> true
            | None -> false
            
        member this.HxPush(url:string) =
            this.SetResponseHeader("HX-Push", url)
            
        member this.Html(html:string) =
            this.Content(html, MediaTypeNames.Text.Html)
