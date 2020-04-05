namespace Post

open Shared

type AirtableConfig =
    { Url: string
      ApiKey: string } with
    static member Load() =
        { Url = Some "http://localhost" |> Env.getEnv "AIRTABLE_URI"
          ApiKey = Some "test" |> Env.getSecret "airtable" "api-key" }
