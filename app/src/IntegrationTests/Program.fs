open canopy.classic
open canopy.runner.classic
open canopy.types
open IntegrationTests
open OpenQA.Selenium.Chrome

let canopyConfig = CanopyConfig.Load()
let clientUrl = canopyConfig.ClientUrl
let screenshotDir = canopyConfig.ScreenshotDir

canopy.configuration.chromeDir <- canopyConfig.DriverDir
canopy.configuration.webdriverPort <- Some canopyConfig.DriverPort
canopy.configuration.failScreenshotPath <- canopyConfig.ScreenshotDir
canopy.configuration.failureScreenshotsEnabled <- true

[<RequireQualifiedAccess>]
type StartMode =
    | Headfull
    | Headless
    | Remote

let startBrowser (startMode:StartMode) =
    let browserStartMode =
        let chromeOptions = ChromeOptions()
        chromeOptions.AddArgument("--no-sandbox")
        match startMode with
        | StartMode.Headfull ->
            ChromeWithOptions chromeOptions
        | StartMode.Headless ->
            chromeOptions.AddArgument("--headless")
            ChromeWithOptions chromeOptions
        | StartMode.Remote ->
            chromeOptions.AddArgument("--headless")
            Remote(canopyConfig.DriverUrl, chromeOptions.ToCapabilities())
    start browserStartMode
    pin Left 
    resize (1000, 600)


let startApp () =
    url clientUrl
    waitForElement "#app"

"test app" &&& fun _ ->
    startApp()
    describe "should be on home page"
    on clientUrl
    waitForElement "#win-dev"
    screenshot screenshotDir "home" |> ignore
    describe "should be two posts"
    count ".post" 2
    describe "should navigate to 'win-dev' post"
    click "#win-dev"
    describe "should be on 'win-dev' post"
    on $"{clientUrl}/win-dev"
    waitForElement "#title"
    screenshot screenshotDir "win-dev" |> ignore
    let expectedTitle = "Windows Development Environment"
    describe $"title should be '{expectedTitle}'"
    "#title" == expectedTitle

[<EntryPoint>]
let main argv =
    printfn "%A" canopyConfig
    let startMode =
        match argv with
        | [|"--headless"|] -> StartMode.Headless
        | [|"--remote"|] -> StartMode.Remote
        | _ -> StartMode.Headfull
    let mutable failed = false
    try
        startBrowser startMode
        run()
        onFail (fun _ -> failed <- true)
        quit()
        if failed then 1 else 0
    with ex ->
        printfn "Error! %A" ex
        quit()
        1
