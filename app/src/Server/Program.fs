open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
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
    Log.Information("Logging at {Url}", config.SeqConfig.Url)

    let configureServices (serviceCollection:IServiceCollection) =
        if config.CI then
            serviceCollection.AddSingleton<IPostClient, MockPostClient>() |> ignore
        else
            serviceCollection.AddSingleton<IPostClient, AirtablePostClient>(fun _ -> AirtablePostClient(config.AirtableConfig)) |> ignore
        serviceCollection.AddSpaStaticFiles(fun config -> config.RootPath <- "wwwroot")
        serviceCollection.AddGiraffe() |> ignore

    let configureApp (appBuilder:IApplicationBuilder) =
        appBuilder.UseDefaultFiles() |> ignore
        appBuilder.UseSpaStaticFiles()
        appBuilder.UseGiraffe Server.HttpHandlers.app
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
