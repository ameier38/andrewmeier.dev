open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Prometheus
open Server.Config
open Server.PostClient
open Serilog
open Serilog.Events

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
            // Add a memory cache service so we can cache the permalink page ids
            builder.Services.AddMemoryCache(fun opts -> opts.SizeLimit <- 1000L) |> ignore
            // Add the Notion configuration which is a dependency of the Notion client
            builder.Services.AddSingleton<NotionConfig>(config.NotionConfig) |> ignore
            // When testing use the mock post client
            if config.AppEnv = AppEnv.Dev then
                builder.Services.AddSingleton<IPostClient,MockPostClient>() |> ignore
            // Otherwise use the live post client
            else
                builder.Services.AddSingleton<IPostClient,LivePostClient>() |> ignore
            builder.Services.AddControllers() |> ignore
            builder.Services.AddHealthChecks() |> ignore
            
            let app = builder.Build()
            // Serve static files from wwwroot folder
            app.UseStaticFiles() |> ignore
            // User Serilog request logging for cleaner logs
            app.UseSerilogRequestLogging() |> ignore
            // Add controller endpoints
            app.MapControllers() |> ignore
            // Add health check endpoints
            app.MapHealthChecks("/healthz") |> ignore
            // Add Prometheus /metrics endpoint
            app.MapMetrics() |> ignore
            // Run the server on the specified host and port
            app.Run(config.ServerConfig.Url)
            0
        with ex ->
            Log.Error(ex, "Error running server")
            1
    finally
        Log.CloseAndFlush()
