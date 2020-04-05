open System.Text.Json
open System.Text.Json.Serialization

[<EntryPoint>]
let main argv =
    let s = """{"query": {}}"""
    let jsonDoc = JsonDocument.Parse(s)
    let query = jsonDoc.RootElement.GetProperty("query").GetRawText()
    printfn "query: %s" query
    0