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
let serverProj = src </> "Server" </> "Server.fsproj"
let testsProj = src </> "Tests" </> "Tests.fsproj"
let screenshotsDir = root </> ".screenshots"

let registerTasks() =

    let clean = BuildTask.create "Clean" [] {
        !! $"{src}/**/bin"
        ++ $"{src}/**/obj"
        ++ $"{src}/**/out"
        -- $"{src}/Build/**"
        |> Seq.map (fun p -> printfn "cleaning: %s" p; p)
        |> Shell.cleanDirs 
    }
    
    BuildTask.create "Restore" [clean] {
        DotNet.restore id sln
    } |> ignore
    
    let watchServer = BuildTask.create "WatchServer" [] {
        let env = Map.ofList [
            "SECRETS_DIR", "/dev/secrets/andrewmeier.dev"
        ]
        let res =
            DotNet.exec
                (fun opts -> { opts with Environment = env })
                "watch" $"--project {serverProj} run"
        if not res.OK then
            failwithf $"{res.Errors}"
    }
    
    let watchTailwind = BuildTask.create "WatchTailwind" [] {
        Npm.run "css:watch" (fun opts -> { opts with WorkingDirectory = src </> "Server" })
    }

    BuildTask.createEmpty "Watch" [watchServer; watchTailwind] |> ignore
    
    BuildTask.create "Test" [] {
        DotNet.test id testsProj
    } |> ignore

    let buildTailwind = BuildTask.create "BuildTailwind" [] {
        Npm.run "css:build" (fun opts -> { opts with WorkingDirectory = src </> "Server" })
    }
    
    BuildTask.create "Publish" [ buildTailwind ] {
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
