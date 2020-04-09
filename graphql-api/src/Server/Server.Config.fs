namespace Server

open Shared

type ServerConfig =
    { Host: string
      Port: int } with
    static member Load() =
        { Host = Some "0.0.0.0" |> Env.getEnv "SERVER_HOST"
          Port = Some "4000" |> Env.getEnv "SERVER_PORT" |> int }

type SeqConfig =
    { Url: string } with
    static member Load() =
        let scheme = Some "http" |> Env.getEnv "SEQ_SCHEME"
        let host = Some "localhost" |> Env.getEnv "SEQ_HOST"
        let port = Some "5341" |> Env.getEnv "SEQ_PORT"
        { Url = sprintf "%s://%s:%s" scheme host port }

type Config =
    { Debug: bool
      ServerConfig: ServerConfig
      SeqConfig: SeqConfig
      AirtableConfig: Post.AirtableConfig } with
    static member Load() =
        { Debug = Some "false" |> Env.getEnv "DEBUG" |> bool.Parse
          ServerConfig = ServerConfig.Load()
          SeqConfig = SeqConfig.Load()
          AirtableConfig = Post.AirtableConfig.Load() }
