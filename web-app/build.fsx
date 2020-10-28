#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.JavaScript
open BlackFox.Fake

let run cmd args workDir =
    CreateProcess.fromRawCommand cmd args
    |> CreateProcess.withWorkingDirectory workDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let clean = BuildTask.create "Clean" [] {
    !! "src/**/bin"
    ++ "src/**/obj"
    ++ ".fable"
    |> Shell.cleanDirs 

    "dist/main.js"
    |> Shell.rm
}

let cleanNode = BuildTask.create "CleanNode" [] {
    "node_modules"
    |> Shell.cleanDir
}

BuildTask.create "Install" [] {
    Npm.install id
}

BuildTask.create "Restore" [clean] {
    !! "src/**/*.fsproj"
    |> Seq.iter (fun proj -> DotNet.restore id proj)
}

BuildTask.create "Serve" [] {
    Npm.run "start" id
}

BuildTask.create "Build" [clean] {
    Npm.run "build" id
}

let _default = BuildTask.createEmpty "Default" []

BuildTask.runOrDefault _default
