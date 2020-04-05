#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
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

BuildTask.create "Scratch" [] {
    DotNet.exec id "run" "-p src/Scratch/Scratch.fsproj"
    |> ignore
}

BuildTask.create "Publish" [clean] {
    let customParams = "/p:PublishSingleFile=true /p:PublishTrimmed=true"
    let runtime = Environment.environVarOrDefault "runtime" "linux-x64"
    DotNet.publish
        (fun args -> 
            { args with
                OutputPath = Some "src/Server/out"
                Runtime = Some runtime
                Common =
                    args.Common
                    |> DotNet.Options.withCustomParams (Some customParams) })
        "src/Server/Server.fsproj"
}

let _default = BuildTask.createEmpty "Default" []

BuildTask.runOrDefault _default
