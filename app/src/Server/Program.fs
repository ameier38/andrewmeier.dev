﻿open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Prometheus
open Server.Config
open Server.PostClient
open Serilog
open Serilog.Events

[<EntryPoint>]
let main _ =
    let config = Config.Load()
    let logger = 
        LoggerConfiguration()
            .MinimumLevel.Is(if config.Debug then LogEventLevel.Debug else LogEventLevel.Information)
            .WriteTo.Console()
            .CreateLogger()
    Log.Logger <- logger
    Log.Debug("Debug mode")
    Log.Debug("{Config}", config)
    
    try
        try
            let builder = WebApplication.CreateBuilder()
            builder.Services.AddSingleton<Config>(fun _ -> config) |> ignore
            builder.Services.AddMemoryCache(fun opts -> opts.SizeLimit <- 1000L) |> ignore
            if config.CI then builder.Services.AddSingleton<IPostClient,MockPostClient>() |> ignore
            else builder.Services.AddSingleton<IPostClient,LivePostClient>() |> ignore
            builder.Services.AddControllers() |> ignore
            builder.Services.AddHealthChecks() |> ignore
            let app = builder.Build()
            app.UseStaticFiles() |> ignore
            app.MapControllers() |> ignore
            app.MapHealthChecks("/healthz") |> ignore
            app.MapMetrics() |> ignore
            app.Run(config.ServerConfig.Url)
            0
        with ex ->
            Log.Error(ex, "Error running server")
            1
    finally
        Log.CloseAndFlush()
