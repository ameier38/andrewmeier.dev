module Server.Config

open System

[<RequireQualifiedAccess>]
type AppEnv =
    | Prod
    | Dev

type ServerConfig =
    { Url:string }
    static member Load() =
        let url = Env.variable "SERVER_URL" "https://0.0.0.0:5000"
        { Url = url }
        
type NotionConfig =
    { DatabaseId:string
      Token:string }
    static member Load() =
        { DatabaseId = Env.variable "NOTION_ARTICLES_DATABASE_ID" ""
          Token = Env.secret "notion" "token" "NOTION_TOKEN" "" }

type Config =
    { AppEnv: AppEnv
      Debug: bool
      ServerConfig: ServerConfig
      NotionConfig:NotionConfig }
    static member Load() =
        { AppEnv = match Env.variable "APP_ENV" "prod" with "prod" -> AppEnv.Prod | _ -> AppEnv.Dev
          Debug = Env.variable "DEBUG" "true" |> Boolean.Parse
          ServerConfig = ServerConfig.Load()
          NotionConfig = NotionConfig.Load() }
