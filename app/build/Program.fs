open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.JavaScript
open BlackFox.Fake
open System.IO

let context = Context.FakeExecutionContext.Create false "build.fsx" []
Context.setExecutionContext (Context.RuntimeContext.Fake context)

let rec findParent (dir:string) (file:string) =
    match Directory.tryFindFirstMatchingFile file dir with
    | Some _ -> dir
    | None -> findParent (Directory.GetParent(dir).FullName) file

let path (parts:string list) = Path.Combine(List.toArray parts)

let root = findParent __SOURCE_DIRECTORY__ "fake.sh"

let clientRoot = path [root; "client"]
let serverRoot = path [root; "server"]
let unitTestsRoot = path [root; "unit-tests"]
let integrationTestsRoot = path [root; "integration-tests"]

let clean = BuildTask.create "Clean" [] {
    !! $"{root}/**/bin"
    ++ $"{root}/**/obj"
    ++ $"{root}/**/out"
    -- $"{root}/build/**"
    -- $"{root}/**/node_modules/**/bin"
    |> Seq.map (fun p -> printfn "cleaning: %s" p; p)
    |> Shell.cleanDirs 

}

let cleanClient = BuildTask.create "CleanClient" [] {
    !! $"{root}/**/*.fs.js"
    |> File.deleteAll
}

let restore = BuildTask.create "Restore" [] {
    !! $"{root}/**/*.fsproj"
    -- $"{root}/build/**"
    -- $"{root}/**/.fable/**"
    |> Seq.iter (DotNet.restore id)
}

let testUnits = BuildTask.create "TestUnits" [] {
    let res = DotNet.exec (fun opts -> { opts with WorkingDirectory = unitTestsRoot }) "run" ""
    if not res.OK then
        failwithf $"{res.Errors}"
}

let startServer = BuildTask.create "StartServer" [] {
    DotNet.exec (fun opts -> { opts with WorkingDirectory = serverRoot}) "watch" "run"
    |> ignore
}

let publishServer = BuildTask.create "PublishServer" [] {
    let runtime = Environment.environVarOrDefault "RUNTIME_ID" "linux-x64"
    Trace.tracefn "Publishing with runtime %s" runtime
    DotNet.publish
        (fun args -> 
            { args with
                OutputPath = Some $"{serverRoot}/out"
                Runtime = Some runtime })
        $"{serverRoot}/Server.fsproj"
}

let installClient = BuildTask.create "InstallClient" [] {
    Npm.install (fun opts -> { opts with WorkingDirectory = clientRoot })
}

let startClient = BuildTask.create "StartClient" [installClient] {
    Npm.run "start" (fun opts -> { opts with WorkingDirectory = clientRoot})
}

let buildClient = BuildTask.create "BuildClient" [installClient] {
    Npm.run "build" (fun opts -> { opts with WorkingDirectory = clientRoot})
}

let empty = BuildTask.createEmpty "Empty" []

[<EntryPoint>]
let main argv =
    let buildTask =
        match argv with
        | [| "Clean" |] -> clean
        | [| "CleanClient" |] -> cleanClient
        | [| "Restore" |] -> restore
        | [| "TestUnits" |] -> testUnits
        | [| "StartServer" |] -> startServer
        | [| "PublishServer" |] -> publishServer
        | [| "StartClient" |] -> startClient
        | [| "BuildClient" |] -> buildClient
        | _ ->
            BuildTask.listAvailable()
            empty
    BuildTask.runOrDefaultApp buildTask
