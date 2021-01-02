open canopy.classic
open canopy.runner.classic
open canopy.types
open IntegrationTests
open OpenQA.Selenium.Chrome

let canopyConfig = CanopyConfig.Load()
let clientUrl = canopyConfig.ClientUrl

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
    describe "should be two posts"
    count ".post" 2
    describe "should navigate to 'about' post"
    click "#about"
    describe "should be on 'about' post"
    on $"{clientUrl}/about"
    describe "title should be 'About'"
    "#title" == "About"

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
