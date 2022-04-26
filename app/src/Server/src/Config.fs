module Server.Config

open System

type ServerConfig =
    { Url:string }
    static member Load() =
        let host = Env.variable "SERVER_HOST" "0.0.0.0"
        let port = Env.variable "SERVER_PORT" "5000" |> int
        { Url = $"http://{host}:{port}" }
        
type NotionConfig =
    { DatabaseId:string
      Token:string }
    static member Load() =
        { DatabaseId = Env.variable "NOTION_DATABASE_ID" ""
          Token = Env.secret "notion" "token" "NOTION_TOKEN" "" }

type Config =
    { Debug: bool
      CI: bool
      ServerConfig: ServerConfig
      NotionConfig:NotionConfig }
    static member Load() =
        { Debug = Env.variable "DEBUG" "true" |> Boolean.Parse
          CI = Env.variable "CI" "false" |> Boolean.Parse
          ServerConfig = ServerConfig.Load()
          NotionConfig = NotionConfig.Load() }
