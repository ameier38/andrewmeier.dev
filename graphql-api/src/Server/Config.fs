namespace Server

open Shared

type ServerConfig =
    { Host: string
      Port: int } with
    static member Load() =
        { Host = Env.getEnv "SERVER_HOST" "0.0.0.0"
          Port = Env.getEnv "SERVER_PORT" "4000" |> int }

type SeqConfig =
    { Url: string } with
    static member Load() =
        let scheme = Env.getEnv "SEQ_SCHEME" "http"
        let host = Env.getEnv "SEQ_HOST" "localhost"
        let port = Env.getEnv "SEQ_PORT" "5341"
        { Url = sprintf "%s://%s:%s" scheme host port }

type AirtableConfig =
    { ApiUrl: string
      ApiKey: string
      BaseId: string } with
    static member Load() =
        let secretName = Env.getEnv "AIRTABLE_SECRET" "airtable"
        let getSecret = Env.getSecret secretName
        { ApiUrl = "https://api.airtable.com/v0"
          ApiKey = getSecret "api-key" "AIRTABLE_API_KEY" "test"
          BaseId = getSecret "base-id" "AIRTABLE_BASE_ID" "test" }

type Config =
    { Debug: bool
      ServerConfig: ServerConfig
      SeqConfig: SeqConfig
      AirtableConfig: AirtableConfig } with
    static member Load() =
        { Debug = Env.getEnv "DEBUG" "true" |> bool.Parse
          ServerConfig = ServerConfig.Load()
          SeqConfig = SeqConfig.Load()
          AirtableConfig = AirtableConfig.Load() }
