open Fable.Remoting.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
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
    if config.Debug then
        Log.Debug("Config {@Config}", config)

    let configureServices (serviceCollection:IServiceCollection) =
        serviceCollection
            .AddRouting()
            .AddHealthChecks() |> ignore
        if config.CI then
            serviceCollection.AddSingleton<IPostClient, MockPostClient>() |> ignore
        else
            serviceCollection.AddSingleton<IPostClient, AirtablePostClient>(fun _ ->
                AirtablePostClient(config.AirtableConfig)) |> ignore

    let configureApp (appBuilder:IApplicationBuilder) =
        appBuilder.UseRemoting(Server.Api.postApi)
        appBuilder
            .UseRouting()
            .UseDefaultFiles()
            .UseStaticFiles()
            .UseEndpoints(fun endpoints ->
                endpoints.MapHealthChecks("/healthz") |> ignore
                endpoints.MapFallbackToFile("index.html") |> ignore)
            |> ignore
        
    WebHostBuilder()
        .UseSerilog()
        .UseKestrel()
        .ConfigureServices(configureServices)
        .Configure(System.Action<IApplicationBuilder> configureApp)
        .UseUrls(config.ServerConfig.Url)
        .Build()
        .Run()
    0
