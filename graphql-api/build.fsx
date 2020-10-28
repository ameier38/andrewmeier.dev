#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open BlackFox.Fake

let clean = BuildTask.create "Clean" [] {
    !! "src/**/bin"
    ++ "src/**/obj"
    ++ "src/**/out"
    |> Shell.cleanDirs 
}

BuildTask.create "Restore" [] {
    !! "src/**/*.fsproj"
    |> Seq.iter (DotNet.restore id)
}

BuildTask.create "Serve" [] {
    DotNet.exec id "run" "-p src/Server/Server.fsproj"
    |> ignore
}

BuildTask.create "Publish" [clean] {
    let runtime =
        if Environment.isLinux then "linux-x64"
        elif Environment.isWindows then "win-x64"
        elif Environment.isMacOS then "osx-x64"
        else failwithf "environment not supported"
    DotNet.publish
        (fun args -> 
            { args with
                OutputPath = Some "src/Server/out"
                Runtime = Some runtime })
        "src/Server/Server.fsproj"
}

let _default = BuildTask.createEmpty "Default" []

BuildTask.runOrDefault _default
