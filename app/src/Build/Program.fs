open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.JavaScript
open BlackFox.Fake

let src = Path.getDirectory __SOURCE_DIRECTORY__
let root = Path.getDirectory src
let sln = root </> "andrewmeier.dev.sln"
let serverProj = src </> "Server" </> "Server.fsproj"
let testsProj = src </> "Tests" </> "Tests.fsproj"

let registerTasks() =

    let watchServer = async {
        let res = DotNet.exec id "watch" $"--project {serverProj} run"
        if not res.OK then failwithf $"{res.Errors}"
    }

    let watchTailwind = async {
        Npm.run "css:watch" (fun opts -> { opts with WorkingDirectory = src </> "Server" })
    }

    BuildTask.create "Watch" [] {
        [watchServer; watchTailwind]
        |> Async.Parallel
        |> Async.Ignore
        |> Async.RunSynchronously
    } |> ignore
    
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
