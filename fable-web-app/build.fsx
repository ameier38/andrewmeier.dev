#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.JavaScript
open BlackFox.Fake

let clean = BuildTask.create "Clean" [] {
    !! "src/**/bin"
    ++ "src/**/obj"
    |> Shell.cleanDirs 
}

BuildTask.create "Restore" [] {
    !! "src/**/*.fsproj"
    |> Seq.iter (fun proj -> DotNet.restore id proj)
}

BuildTask.create "Start" [] {
    Npm.exec "start" id
}

let _default = BuildTask.createEmpty "Default" []

BuildTask.runOrDefault _default
