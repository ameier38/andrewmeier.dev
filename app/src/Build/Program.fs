open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators
open Fake.JavaScript
open BlackFox.Fake

let src = Path.getDirectory __SOURCE_DIRECTORY__
    
let root = Path.getDirectory src

let sln = root </> "andrewmeier.dev.sln"
let clientProj = src </> "Client" </> "src" </> "Client.fsproj"
let serverProj = src </> "Server" </> "Server.fsproj"
let testsProj = src </> "Tests" </> "Tests.fsproj"
let screenshotsDir = root </> ".screenshots"

let registerTasks() =

    let clean = BuildTask.create "Clean" [] {
        !! $"{src}/**/bin"
        ++ $"{src}/**/obj"
        ++ $"{src}/**/out"
        -- $"{src}/Build/**"
        -- $"{src}/**/node_modules/**"
        |> Seq.map (fun p -> printfn "cleaning: %s" p; p)
        |> Shell.cleanDirs 
    }
    
    let cleanClient = BuildTask.create "CleanClient" [] {
        Shell.cleanDir $"{src}/Client/compiled"
    }

    BuildTask.create "Restore" [clean] {
        DotNet.restore id sln
    } |> ignore

    let watchServer =
        async {
            let env = Map.ofList [ "CI", "true" ]
            let res =
                DotNet.exec
                    (fun opts -> { opts with Environment = env })
                    "watch"
                    $"-p {serverProj} run"
            if not res.OK then
                failwithf $"{res.Errors}"
            
        }
    
    BuildTask.create "WatchServer" [] {
        Async.RunSynchronously(watchServer)
    } |> ignore
    
    let installClient = BuildTask.create "InstallClient" [] {
        let clientRoot = src </> "Client"
        Npm.install (fun opts -> { opts with WorkingDirectory = clientRoot })
    }
    
    let watchClient =
        async {
            let clientRoot = src </> "Client"
            Npm.run "start" (fun opts -> { opts with WorkingDirectory = clientRoot})
        }

    BuildTask.create "WatchClient" [installClient] {
        Async.RunSynchronously(watchClient)
    } |> ignore
    
    BuildTask.create "Watch" [installClient] {
        [watchServer; watchClient]
        |> Async.Parallel
        |> Async.Ignore
        |> Async.RunSynchronously
    } |> ignore

    BuildTask.create "BuildClient" [cleanClient; installClient] {
        let clientRoot = src </> "Client"
        Npm.run "build" (fun opts -> { opts with WorkingDirectory = clientRoot})
    } |> ignore

    BuildTask.create "TestUnits" [] {
        let res =
            DotNet.exec
                id
                "run"
                $"-p {testsProj} test-units"
        if not res.OK then
            failwithf $"{res.Errors}"
    } |> ignore
    
    BuildTask.create "TestIntegrations" [] {
        let res =
            DotNet.exec
                (fun opts -> { opts with Environment = Map.ofList [ "SCREENSHOTS_DIR", screenshotsDir ] })
                "run"
                $"-p {testsProj} test-integrations"
        if not res.OK then
            failwithf $"{res.Errors}"
    } |> ignore

    BuildTask.create "TestIntegrationsHeadless" [] {
        let res =
            DotNet.exec
                (fun opts -> { opts with Environment = Map.ofList [ "SCREENSHOTS_DIR", screenshotsDir ] })
                "run"
                $"-p {testsProj} test-integrations --browser-mode headless"
        if not res.OK then
            failwithf $"{res.Errors}"
    } |> ignore

    BuildTask.create "PublishServer" [] {
        let runtime = Environment.environVarOrDefault "RUNTIME_ID" "linux-x64"
        let serverRoot = Path.getDirectory serverProj
        Trace.tracefn "Publishing with runtime %s" runtime
        DotNet.publish
            (fun args ->
                { args with
                    OutputPath = Some $"%s{serverRoot}/out"
                    Runtime = Some runtime
                    SelfContained = Some false })
            serverProj
    } |> ignore

[<EntryPoint>]
let main argv =
    BuildTask.setupContextFromArgv argv
    registerTasks()
    BuildTask.runOrListApp() 
