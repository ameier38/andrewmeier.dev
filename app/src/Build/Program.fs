open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators
open Fake.JavaScript
open BlackFox.Fake

let src =
    __SOURCE_DIRECTORY__ // Build
    |> Path.getDirectory // src
    
let root = Path.getDirectory src

let sln = root </> "andrewmeier.dev.sln"
let clientProj = src </> "Client" </> "src" </> "Client.fsproj"
let serverProj = src </> "Server" </> "Server.fsproj"
let unitTestsProj = src </> "UnitTests" </> "UnitTests.fsproj"
let integrationTestsProj = src </> "IntegrationTests" </> "IntegrationTests.fsproj"

let registerTasks() =

    let clean = BuildTask.create "Clean" [] {
        !! $"{src}/**/bin"
        ++ $"{src}/**/obj"
        ++ $"{src}/**/out"
        -- $"{src}/Build/**"
        -- $"{src}/**/node_modules/**/bin"
        |> Seq.map (fun p -> printfn "cleaning: %s" p; p)
        |> Shell.cleanDirs 
    }
    
    let cleanClient = BuildTask.create "CleanClient" [] {
        Shell.cleanDir $"{src}/Client/compiled"
    }

    BuildTask.create "Restore" [clean] {
        DotNet.restore id sln
    } |> ignore

    BuildTask.create "TestUnits" [] {
        let res =
            DotNet.exec
                id
                "run"
                $"-p {unitTestsProj}"
        if not res.OK then
            failwithf $"{res.Errors}"
    } |> ignore

    BuildTask.create "WatchServer" [] {
        let env = Map.ofList [ "CI", "true" ]
        let res =
            DotNet.exec
                (fun opts -> { opts with Environment = env })
                "watch"
                $"-p {serverProj} run"
        if not res.OK then
            failwithf $"{res.Errors}"
    } |> ignore

    BuildTask.create "TestIntegrations" [] {
        let res =
            DotNet.exec
                id
                "run"
                $"-p {integrationTestsProj}"
        if not res.OK then
            failwithf $"{res.Errors}"
    } |> ignore

    BuildTask.create "TestIntegrationsHeadless" [] {
        let res =
            DotNet.exec
                id
                "run"
                $"-p {integrationTestsProj} --headless"
        if not res.OK then
            failwithf $"{res.Errors}"
    } |> ignore

    let publish projPath =
        let runtime = Environment.environVarOrDefault "RUNTIME_ID" "linux-x64"
        let projRoot = Path.getDirectory projPath
        Trace.tracefn "Publishing with runtime %s" runtime
        DotNet.publish
            (fun args -> 
                { args with
                    OutputPath = Some $"%s{projRoot}/out"
                    Runtime = Some runtime })
            $"%s{projPath}"

    BuildTask.create "PublishServer" [] {
        publish serverProj
    } |> ignore

    BuildTask.create "PublishIntegrationTests" [] {
        publish integrationTestsProj
    } |> ignore

    let installClient = BuildTask.create "InstallClient" [] {
        let clientRoot = src </> "Client"
        Npm.install (fun opts -> { opts with WorkingDirectory = clientRoot })
    }

    BuildTask.create "StartClient" [installClient] {
        let clientRoot = src </> "Client"
        Npm.run "start" (fun opts -> { opts with WorkingDirectory = clientRoot})
    } |> ignore

    BuildTask.create "BuildClient" [cleanClient; installClient] {
        let clientRoot = src </> "Client"
        Npm.run "build" (fun opts -> { opts with WorkingDirectory = clientRoot})
    } |> ignore

[<EntryPoint>]
let main argv =
    BuildTask.setupContextFromArgv argv
    registerTasks()
    BuildTask.runOrListApp() 
