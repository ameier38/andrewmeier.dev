namespace IntegrationTests

open Shared
open System
open System.IO

type CanopyConfig =
    { ClientUrl: string
      ChromeDriverDir: string
      ScreenshotsDir: string }
    static member Load() =
        let clientScheme = Env.variable "CLIENT_SCHEME" "http"
        let clientHost = Env.variable "CLIENT_HOST" "localhost"
        let clientPort = Env.variable "CLIENT_PORT" "5000" |> int
        { ClientUrl = $"{clientScheme}://{clientHost}:{clientPort}"
          ChromeDriverDir = Env.variable "CHROME_DRIVER_DIR" AppContext.BaseDirectory
          ScreenshotsDir = Env.variable "SCREENSHOTS_DIR" (Path.Join(AppContext.BaseDirectory, "screenshots")) }
