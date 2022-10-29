open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Prometheus
open Server.Config
open Server.PostClient
open Serilog
open Serilog.Events

let configureServices (config:Config) (services:IServiceCollection) =
    // Add a memory cache service so we can cache the permalink page ids
    services.AddMemoryCache(fun opts -> opts.SizeLimit <- 1000L) |> ignore
    // Add the Notion configuration which is a dependency of the Notion client
    services.AddSingleton<NotionConfig>(config.NotionConfig) |> ignore
    match config.AppEnv with
    // When testing use the mock post client
    | AppEnv.Dev -> services.AddSingleton<IPostClient,MockPostClient>() |> ignore
    // Otherwise use the live post client
    | _ -> services.AddSingleton<IPostClient,LivePostClient>() |> ignore
    services.AddHealthChecks() |> ignore
    services.AddGiraffe() |> ignore
    
let configureApp (app:WebApplication) =
    // Serve static files from wwwroot folder
    app.UseStaticFiles() |> ignore
    // User Serilog request logging for cleaner logs
    app.UseSerilogRequestLogging() |> ignore
    // Add health check endpoints
    app.MapHealthChecks("/healthz") |> ignore
    // Add Prometheus /metrics endpoint
    app.MapMetrics() |> ignore
    // Add application routes
    app.UseGiraffe(Server.PostHandler.postApp)

[<EntryPoint>]
let main _ =
    // Load the config from Config.fs
    let config = Config.Load()
    // Configure the Serilog logger
    let logger = 
        LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Is(if config.Debug then LogEventLevel.Debug else LogEventLevel.Information)
            .WriteTo.Console()
            .CreateLogger()
    Log.Logger <- logger
    Log.Debug("Debug mode")
    Log.Debug("{Config}", config)
    
    try
        try
            let builder = WebApplication.CreateBuilder()
            builder.Host.UseSerilog() |> ignore
            configureServices config builder.Services
            
            let app = builder.Build()
            configureApp app
            // Run the server on the specified host and port
            app.Run(config.ServerConfig.Url)
            0
        with ex ->
            Log.Error(ex, "Error running server")
            1
    finally
        Log.CloseAndFlush()
