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

let clientRoot = src </> "Client"
let serverRoot = src </> "Server"
let unitTestsRoot = src </> "UnitTests"
let integrationTestsRoot = src </> "IntegrationTests"

let registerTasks() =

    BuildTask.create "Clean" [] {
        !! $"{src}/**/bin"
        ++ $"{src}/**/obj"
        ++ $"{src}/**/out"
        -- $"{src}/Build/**"
        -- $"{src}/**/node_modules/**/bin"
        |> Seq.map (fun p -> printfn "cleaning: %s" p; p)
        |> Shell.cleanDirs 
    } |> ignore

    BuildTask.create "CleanClient" [] {
        !! $"{src}/**/*.fs.js"
        |> File.deleteAll
    } |> ignore

    BuildTask.create "Restore" [] {
        !! $"{src}/**/*.fsproj"
        -- $"{src}/Build/**"
        -- $"{src}/**/.fable/**"
        |> Seq.iter (DotNet.restore id)
    } |> ignore

    let dotnet workingDir env command args =
        let res = DotNet.exec (fun opts ->
            { opts with
                WorkingDirectory = workingDir
                Environment = env }) command args
        if not res.OK then
            failwithf $"{res.Errors}"

    BuildTask.create "TestUnits" [] {
        let res =
            DotNet.exec
                (fun opts -> { opts with WorkingDirectory = unitTestsRoot })
                "run"
                $"-p {unitTestsRoot}/UnitTests.fsproj"
        if not res.OK then
            failwithf $"{res.Errors}"
    } |> ignore

    BuildTask.create "StartServer" [] {
        let env = Map.ofList [ "CI", "true" ]
        dotnet serverRoot env "watch" "run"
    } |> ignore

    BuildTask.create "TestIntegrations" [] {
        dotnet integrationTestsRoot Map.empty "run" ""
    } |> ignore

    BuildTask.create "TestIntegrationsHeadless" [] {
        dotnet integrationTestsRoot Map.empty "run" "--headless"
    } |> ignore

    let publish projRoot =
        let runtime = Environment.environVarOrDefault "RUNTIME_ID" "linux-x64"
        Trace.tracefn "Publishing with runtime %s" runtime
        DotNet.publish
            (fun args -> 
                { args with
                    OutputPath = Some $"%s{projRoot}/out"
                    Runtime = Some runtime })
            $"%s{projRoot}"

    BuildTask.create "PublishServer" [] {
        publish serverRoot
    } |> ignore

    BuildTask.create "PublishIntegrationTests" [] {
        publish integrationTestsRoot
    } |> ignore

    let installClient = BuildTask.create "InstallClient" [] {
        Npm.install (fun opts -> { opts with WorkingDirectory = clientRoot })
    }

    BuildTask.create "StartClient" [installClient] {
        Npm.run "start" (fun opts -> { opts with WorkingDirectory = clientRoot})
    } |> ignore

    BuildTask.create "BuildClient" [installClient] {
        Npm.run "build" (fun opts -> { opts with WorkingDirectory = clientRoot})
    } |> ignore

[<EntryPoint>]
let main argv =
    BuildTask.setupContextFromArgv argv
    registerTasks()
    BuildTask.runOrListApp() 
