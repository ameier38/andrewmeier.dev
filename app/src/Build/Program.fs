open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open System.Threading.Tasks

let src = Path.getDirectory __SOURCE_DIRECTORY__
let root = Path.getDirectory src
let sln = root </> "andrewmeier.dev.sln"
let serverDir = src </> "Server"
let testsDir = src </> "Tests"

let tailwindcss workDir args =
    CreateProcess.fromRawCommand "tailwindcss" args
    |> CreateProcess.withWorkingDirectory workDir
    |> CreateProcess.ensureExitCode
    |> Proc.start

let inline (==>!) x y = x ==> y |> ignore

let registerTargets() =
    
    Target.create "Watch" <| fun _ ->
        let watchCss = tailwindcss serverDir [ "--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--watch" ]
        let watchServer = task {
            let res = DotNet.exec id "watch" $"--project {serverDir} run"
            if not res.OK then failwith $"Error: {res.Errors}"
        }
        Task.WaitAny(watchCss, watchServer) |> ignore
        
    Target.create "Test" <| fun _ ->
        DotNet.test id testsDir
        
    Target.create "BuildCss" <| fun _ ->
        let buildCss = tailwindcss serverDir [ "--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--minify" ]
        buildCss.Wait()
        
    Target.create "Publish" <| fun _ ->
        let runtime = Environment.environVarOrDefault "RUNTIME_ID" "linux-x64"
        DotNet.publish
            (fun opts ->
                { opts with
                    OutputPath = Some $"{serverDir}/out"
                    Runtime = Some runtime
                    SelfContained = Some false })
            serverDir
            
    Target.create "Default" <| fun _ ->
        Target.listAvailable()
        
    "BuildCss" ==>! "Publish"
    
[<EntryPoint>]
let main argv =
    argv
    |> Array.toList
    |> Context.FakeExecutionContext.Create false "build.fsx"
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext
    registerTargets()
    Target.runOrDefaultWithArguments "Default"
    0
