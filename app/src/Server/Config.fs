namespace Server

open Shared

type ServerConfig =
    { Url: string }
    static member Load() =
        let host = Env.getEnv "SERVER_HOST" "0.0.0.0"
        let port = Env.getEnv "SERVER_PORT" "5000" |> int
        { Url = $"http://{host}:{port}" }

type SeqConfig =
    { Url: string }
    static member Load() =
        let scheme = Env.getEnv "SEQ_SCHEME" "http"
        let host = Env.getEnv "SEQ_HOST" "localhost"
        let port = Env.getEnv "SEQ_PORT" "5341"
        { Url = sprintf "%s://%s:%s" scheme host port }

type AirtableConfig =
    { ApiUrl: string
      ApiKey: string
      BaseId: string }
    static member Load() =
        let secretName = Env.getEnv "AIRTABLE_SECRET" "airtable"
        let getSecret = Env.getSecret secretName
        { ApiUrl = "https://api.airtable.com/v0"
          ApiKey = getSecret "api-key" "AIRTABLE_API_KEY" "test"
          BaseId = getSecret "base-id" "AIRTABLE_BASE_ID" "test" }

type Config =
    { Debug: bool
      CI: bool
      ServerConfig: ServerConfig
      SeqConfig: SeqConfig
      AirtableConfig: AirtableConfig }
    static member Load() =
        { Debug = Env.getEnv "DEBUG" "true" |> bool.Parse
          CI = Env.getEnv "CI" "false" |> bool.Parse
          ServerConfig = ServerConfig.Load()
          SeqConfig = SeqConfig.Load()
          AirtableConfig = AirtableConfig.Load() }
