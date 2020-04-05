namespace Post

open Shared

type AirtableConfig =
    { Url: string
      ApiKey: string } with
    static member Load() =
        { Url = Some "http://localhost" |> Env.getSecret "airtable" "url"
          ApiKey = Some "test" |> Env.getSecret "airtable" "api-key" }
