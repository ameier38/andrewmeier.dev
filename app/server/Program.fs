open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Giraffe
open Markdig
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.SpaServices
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Serilog
open Serilog.Events


[<EntryPoint>]
let main _ =
    let config = Server.Config.Load()
    let logger = 
        LoggerConfiguration()
            .MinimumLevel.Is(if config.Debug then LogEventLevel.Debug else LogEventLevel.Information)
            .WriteTo.Console()
            .WriteTo.Seq(config.SeqConfig.Url)
            .CreateLogger()
    Log.Logger <- logger
    Log.Debug("Debug mode")
    Log.Information("Logging at {Url}", config.SeqConfig.Url)

    let markdownPipeline = 
        MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseEmojiAndSmiley()
            .UseMathematics()
            .Build()

    let airtable =
        if config.Debug then Server.Airtable.MockPostClient() :> Server.Airtable.IPostClient
        else Server.Airtable.AirtablePostClient(config.AirtableConfig) :> Server.Airtable.IPostClient

    let routeBuilder (typeName:string) (methodName:string) = $"/api/{typeName}/{methodName}"

    let api: HttpHandler =
        Remoting.createApi()
        |> Remoting.fromValue (Server.Api.postApi markdownPipeline airtable)
        |> Remoting.withRouteBuilder routeBuilder
        |> Remoting.buildHttpHandler

    let app = choose [
        GET >=> route "/healthz" >=> Successful.OK "Healthy!"
        api
    ]

    let configureServices (serviceCollection:IServiceCollection) =
        serviceCollection.AddSpaStaticFiles(fun config -> config.RootPath <- "wwwroot")
        serviceCollection.AddGiraffe() |> ignore

    let configureApp (appBuilder:IApplicationBuilder) =
        appBuilder.UseDefaultFiles() |> ignore
        appBuilder.UseSpaStaticFiles()
        appBuilder.UseGiraffe app
        appBuilder.UseSpa(fun _ -> ())

    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(configureApp)
                .ConfigureServices(configureServices)
                .UseUrls(config.ServerConfig.Url)
                |> ignore)
        .Build()
        .Run()
    0
