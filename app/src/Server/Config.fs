namespace Server

open System

type ServerConfig =
    { Url: string }
    static member Load() =
        let host = Env.variable "SERVER_HOST" "0.0.0.0"
        let port = Env.variable "SERVER_PORT" "5000" |> int
        { Url = $"http://{host}:{port}" }

type SeqConfig =
    { Url: string }
    static member Load() =
        let scheme = Env.variable "SEQ_SCHEME" "http"
        let host = Env.variable "SEQ_HOST" "localhost"
        let port = Env.variable "SEQ_PORT" "5341"
        { Url = $"{scheme}://{host}:{port}" }

type AirtableConfig =
    { ApiUrl: string
      ApiKey: string
      BaseId: string }
    static member Load() =
        let secretName = Env.variable "AIRTABLE_SECRET" "airtable"
        { ApiUrl = "https://api.airtable.com/v0"
          ApiKey = Env.secret secretName "api-key" "AIRTABLE_API_KEY" "test"
          BaseId = Env.secret secretName "base-id" "AIRTABLE_BASE_ID" "test" }

type Config =
    { AppName: string
      AppEnv: AppEnv
      Debug: bool
      ServerConfig: ServerConfig
      SeqConfig: SeqConfig
      AirtableConfig: AirtableConfig }
    static member Load() =
        { AppName = Env.variable "APP_NAME" "andrewmeier.dev"
          AppEnv = match Env.variable "APP_ENV" "DEV" with "PROD" -> AppEnv.Prod | _ -> AppEnv.Dev
          Debug = Env.variable "DEBUG" "true" |> Boolean.Parse
          ServerConfig = ServerConfig.Load()
          SeqConfig = SeqConfig.Load()
          AirtableConfig = AirtableConfig.Load() }
