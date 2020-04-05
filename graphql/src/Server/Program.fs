open FSharp.Data
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.Serialization
open Server
open Serilog
open Serilog.Events
open Suave
open Suave.Filters
open Suave.Operators
open System.Text

type Parser<'T>(executor:GraphQL.Executor<'T>) =
    let jsonOptions = JsonSerializerSettings(ContractResolver = CamelCasePropertyNamesContractResolver())
    do jsonOptions.Converters.Add(GraphQLQueryConverter(executor))

    // ref: https://graphql.org/learn/serving-over-http/#post-request
    member _.ParseRequest(rawBody:byte[]) =
        let strBody = Encoding.UTF8.GetString(rawBody)
        JsonConvert.DeserializeObject<GraphQLQuery>(strBody, jsonOptions)

    // ref: https://graphql.org/learn/serving-over-http/#response
    member _.ParseResponse(res:GraphQL.Execution.GQLResponse) =
        match res.Content with
        | GraphQL.Execution.Direct (data, errors) ->
            match errors with
            | [] -> JsonConvert.SerializeObject(data)
            | errors -> failwithf "%A" errors
        | _ -> failwithf "only direct queries are supported"

let configureRequest = 
    Writers.setHeader "Access-Control-Allow-Headers" "Content-Type"
    >=> Writers.setHeader "Content-Type" "application/json"
    >=> Writers.setMimeType "application/json"

let graphql 
    (parser:Parser<Root.Root>)
    (executor:GraphQL.Executor<Root.Root>): WebPart =
    fun httpCtx ->
        async {
            try
                let body = httpCtx.request.rawForm
                let { ExecutionPlan = plan; Variables = variables } = parser.ParseRequest(body)
                let! gqlResp = 
                    executor.AsyncExecute(
                        executionPlan=plan,
                        variables=variables)
                let sendResp = parser.ParseResponse(gqlResp) |> Successful.OK
                return! sendResp httpCtx
            with ex ->
                Log.Error("Error: {@Exception}", ex)
                let sendResp =
                    {| data = ex.ToString() |}
                    |> JsonConvert.SerializeObject
                    |> ServerErrors.INTERNAL_ERROR
                return! sendResp httpCtx
        }

[<EntryPoint>]
let main _ =
    let config = Config.Load()
    let logger = 
        LoggerConfiguration()
            .MinimumLevel.Is(if config.Debug then LogEventLevel.Debug else LogEventLevel.Information)
            .WriteTo.Console()
            .WriteTo.Seq(config.SeqConfig.Url)
            .CreateLogger()
    Log.Logger <- logger
    Log.Information("logging at {SeqUrl}", config.SeqConfig.Url)
    let postClient = Post.PostClient(config.AirtableConfig)
    let query = Root.Query postClient
    let schema = GraphQL.Schema(query)
    let executor = GraphQL.Executor(schema)
    let parser = Parser(executor)
    let suaveConfig = 
        { defaultConfig 
            with bindings = [ HttpBinding.createSimple HTTP config.ServerConfig.Host config.ServerConfig.Port ]}
    let api = choose [
        path "/health" >=> Successful.OK "Hi!"
        configureRequest >=> graphql parser executor
    ]
    Log.Information("starting server...", config.SeqConfig.Url)
    startWebServer suaveConfig api
    Log.Information("serving at {Host}:{Port}", config.ServerConfig.Host, config.ServerConfig.Port)
    0
