open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
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
            let configureLogger (_ctx:HostBuilderContext) (lc:LoggerConfiguration) = lc.WriteTo.Console() |> ignore
            builder.Host.UseSerilog(System.Action<HostBuilderContext,LoggerConfiguration>(configureLogger)) |> ignore
            builder.Services.AddMemoryCache(fun opts -> opts.SizeLimit <- 1000L) |> ignore
            builder.Services.AddSingleton<NotionConfig>(config.NotionConfig) |> ignore
            if config.AppEnv = AppEnv.Dev then
                builder.Services.AddSingleton<IPostClient,MockPostClient>() |> ignore
            else
                builder.Services.AddSingleton<IPostClient,LivePostClient>() |> ignore
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
