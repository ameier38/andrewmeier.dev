module Tests.IntegrationTests

open canopy.classic
open canopy.runner.classic
open canopy.types
open IntegrationTests
open OpenQA.Selenium.Chrome

[<RequireQualifiedAccess>]
type BrowserMode =
    | Local
    | Headless
    | Remote

let configureCanopy (config:CanopyConfig) =
    canopy.configuration.chromeDir <- config.DriverDir
    canopy.configuration.webdriverPort <- Some config.DriverPort
    canopy.configuration.failScreenshotPath <- config.ScreenshotDir
    canopy.configuration.failureScreenshotsEnabled <- true

let startBrowser (browserMode:BrowserMode, config:CanopyConfig) =
    let browserStartMode =
        let chromeOptions = ChromeOptions()
        chromeOptions.AddArgument("--no-sandbox")
        match browserMode with
        | BrowserMode.Local ->
            ChromeWithOptions chromeOptions
        | BrowserMode.Headless ->
            chromeOptions.AddArgument("--headless")
            ChromeWithOptions chromeOptions
        | BrowserMode.Remote ->
            chromeOptions.AddArgument("--headless")
            Remote(config.DriverUrl, chromeOptions.ToCapabilities())
    start browserStartMode
    pin Left 
    resize (1000, 600)

let startApp (config:CanopyConfig) =
    let clientUrl = config.ClientUrl
    url clientUrl
    waitForElement "#app"

let registerTestApp (config:CanopyConfig) =
    "test app" &&& fun () ->
        startApp config
        describe "should be on home page"
        on config.ClientUrl
        waitForElement "#win-dev"
        screenshot config.ScreenshotDir "home" |> ignore
        describe "should be two posts"
        count ".post-item" 2
        describe "post item should have summary"
        "#win-dev > .post-summary" == "Set up a Window's machine for development."
        describe "should navigate to 'win-dev' post"
        click "#win-dev"
        describe "should be on 'win-dev' post"
        on $"{config.ClientUrl}/win-dev"
        waitForElement "#title"
        screenshot config.ScreenshotDir "win-dev" |> ignore
        let expectedTitle = "Windows Development Environment"
        describe $"title should be '{expectedTitle}'"
        "#title" == expectedTitle
        describe "going directly to url should work"
        url config.ClientUrl
        waitForElement "#win-dev"
        url $"{config.ClientUrl}/win-dev"
        waitForElement "#title"
        "#title" == expectedTitle
    
let run (browserMode:BrowserMode) =
    let mutable failed = false
    try
        let config = CanopyConfig.Load()
        registerTestApp config
        startBrowser (browserMode, config)
        run()
        onFail (fun _ -> failed <- true)
        quit()
        if failed then 1 else 0
    with ex ->
        printfn $"Error! {ex}"
        quit()
        1
