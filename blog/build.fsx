#load ".fake/build.fsx/intellisense.fsx"
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.JavaScript
open BlackFox.Fake

let clean = BuildTask.create "Clean" [] {
    !! "src/**/bin"
    ++ "src/**/obj"
    |> Shell.cleanDirs 
}

BuildTask.create "Restore" [] {
    DotNet.restore id "src/App.fsproj"
}

BuildTask.create "Install" [] {
    Npm.install id
}

BuildTask.create "Build" [clean] {
    Npm.exec "run build" id
}

BuildTask.create "Start" [] {
    Npm.exec "run start" id
}

let _default = BuildTask.createEmpty "Default" []

BuildTask.runOrDefault _default
