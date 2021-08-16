open Fable.Remoting.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Prometheus
open Server.PostClient
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
    Log.Debug("{Config}", config)

    let configureServices (serviceCollection:IServiceCollection) =
        serviceCollection
            .AddRouting()
            .AddHealthChecks() |> ignore
        match config.AppEnv with
        | AppEnv.Dev ->
            serviceCollection.AddSingleton<IPostClient, MockPostClient>() |> ignore
        | AppEnv.Prod ->
            serviceCollection.AddSingleton<IPostClient, AirtablePostClient>(fun _ ->
                AirtablePostClient(config.AirtableConfig)) |> ignore

    let configureApp (appBuilder:IApplicationBuilder) =
        appBuilder.UseRemoting(Server.Api.postApi)
        appBuilder
            .UseRouting()
            .UseHttpMetrics()
            .UseDefaultFiles()
            .UseStaticFiles()
            .UseEndpoints(fun endpoints ->
                endpoints.MapHealthChecks("/healthz") |> ignore
                endpoints.MapFallbackToFile("index.html") |> ignore
                endpoints.MapMetrics() |> ignore)
            |> ignore
        
    try
        try
            WebHostBuilder()
                .UseSerilog()
                .UseKestrel()
                .ConfigureServices(configureServices)
                .Configure(System.Action<IApplicationBuilder> configureApp)
                .UseUrls(config.ServerConfig.Url)
                .Build()
                .Run()
            0
        with ex ->
            Log.Error(ex, "Error running server")
            1
    finally
        Log.CloseAndFlush()
