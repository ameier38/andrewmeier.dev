---
layout: post
title:  How to FAKE
cover: /assets/images/how-to-fake/cover.png
permalink: how-to-fake
date: 2019-01-05 12:08:00 -0400
updated: 2019-01-06 08:06:00 -0400
categories: 
  - F#
  - FAKE
comments: true
---

Get started with [FAKE](https://fake.build).

## What is FAKE?
FAKE is a domain-specific language (DSL) for build tasks and more
built using F#. Find more information on the [FAKE homepage](https://fake.build).

Some things FAKE can do:
- clean directories (e.g. build, packages)
- compile your application
- download a file from the internet
- run Git commands

## Environment set up (Windows)
Install [Scoop](https://andrewmeier.dev/win-dev#scoop)

Install the .NET Core CLI.
```shell
scoop install dotnet-sdk
```

Install FAKE globally.
```shell
dotnet tool install fake-cli -g
```

Verify the installation.
```shell
fake --help
```

## Create a FAKE script
Create new directory for your project.
```shell
mkdir fake-tutorial
cd fake-tutorial
```

Add the initial `build.fsx` script with the following content:
```fsharp
// installs the FAKE dependencies
#r "paket:
nuget Fake.Core.Target //"
// loads the intellisense script for IDE support
#load "./.fake/build.fsx/intellisense.fsx"

// imports the core FAKE library
open Fake.Core

// defines a target which just logs a message
Target.create "Default" (fun _ ->
    Trace.trace "Hello World")

// runs the specified target
Target.runOrDefault "Default"
```

Run the build script.
```shell
fake build
```
> By default `fake build` looks for a `build.fsx` folder in
the current directory. After running this command you should
see a `.fake` directory created.

Congrats! You ran your first FAKE script! :thumbsup:

## Use FAKE to run an F# test script.
Add a `paket.dependencies` file with the following.
```markup
source https://api.nuget.org/v3/index.json

nuget Expecto
```
> [Expecto](https://github.com/haf/expecto) 
is a test library for F#.

Create a `test.fsx` file in your project directory with the following content:
```fsharp
#r @"packages\Expecto\lib\netstandard2.0\Expecto.dll"
open Expecto

let simpleTest =
    testCase "simple test" <| fun () ->
        let expected = 4
        let actual = 2 + 2
        Expect.equal expected actual "2+2=4"

runTests defaultConfig simpleTest |> exit
```
> Check out the [Expecto documentation](https://github.com/haf/expecto#running-tests)
for more information about running Expecto tests.

Next, replace the contents of the `build.fsx` script with the following:
```fsharp
#r "paket:
nuget Fake.Net.Http
nuget Fake.DotNet.Fsi
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Net
open Fake.DotNet
open System.IO

let paketEndpoint = "https://github.com/fsprojects/Paket/releases/download/5.194.4/paket.exe"
let paketExe = Path.Combine(__SOURCE_DIRECTORY__, ".paket", "paket.exe")

Target.create "Install" (fun _ ->
    if not (File.Exists paketExe) then
        Trace.trace "downloading Paket"
        Http.downloadFile paketExe paketEndpoint
        |> ignore
    else
        Trace.trace "Paket already exists"
    Trace.trace "Installing dependencies"
    Command.RawCommand(paketExe, Arguments.OfArgs ["install"])
    |> CreateProcess.fromCommand
    |> CreateProcess.withFramework
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore)

Target.create "Test" (fun _ ->
    let (exitCode, messages) = 
        Fsi.exec 
            // profile configuration
            (fun p -> { p with TargetProfile = Fsi.Profile.NetStandard } ) 
            // script to run
            "test.fsx" 
            // script arguments
            []
    match exitCode with
    | 0 -> 
        messages
        |> List.iter Trace.trace
    | _ -> 
        messages
        |> List.iter Trace.traceError
        failwith "Error!")

open Fake.Core.TargetOperators

"Install"
 ==> "Test"

Target.runOrDefault "Test"
```

:grimacing: Yea, I know. A lot going on here. Lets break it down:

At the top of the script we added the `Fake.Net.Http` 
and the `Fake.DotNet.Fsi` libraries. This is so we can download 
the `paket.exe` and run the F# script respectively.
```fsharp
#r "paket:
nuget Fake.Net.Http
nuget Fake.DotNet.Fsi
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"
```

Next, we import the libraries which we will use
in the rest of the script.
```fsharp
open Fake.Core
open Fake.Net
open Fake.DotNet
open System.IO
```

Next, we create a target which will download the `paket.exe`
file if it does not exist, and then install the dependencies
listed in the `paket.dependencies` file.
```fsharp
let paketEndpoint = "https://github.com/fsprojects/Paket/releases/download/5.194.4/paket.exe"
let paketExe = Path.Combine(__SOURCE_DIRECTORY__, ".paket", "paket.exe")

Target.create "Install" (fun _ ->
    if not (File.Exists paketExe) then
        Trace.trace "downloading Paket"
        Http.downloadFile paketExe paketEndpoint
        |> ignore
    else
        Trace.trace "Paket already exists"
    Trace.trace "Installing dependencies"
    // run "paket.exe install"
    Command.RawCommand(paketExe, Arguments.OfArgs ["install"])
    |> CreateProcess.fromCommand
    // use mono if linux
    |> CreateProcess.withFramework
    // throw an error if the process fails
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore)
```
> Read more about running processes in FAKE 
[here](https://fake.build/core-process.html).

Next, we create a target which will run the `test.fsx` script.
```fsharp
Target.create "Test" (fun _ ->
    let (exitCode, messages) = 
        Fsi.exec 
            // profile configuration
            (fun p -> { p with TargetProfile = Fsi.Profile.NetStandard } ) 
            // script to run
            "test.fsx" 
            // script arguments
            []
    let traceMessages = String.concat ";" >> Trace.trace
    match exitCode with
    | 0 -> 
        traceMessages messages
    | _ -> 
        traceMessages messages
        failwith "Error!")
```
> The `Fsi` module is located in the `Fake.DotNet` library
and the `Fsi.exec` is a function used to execute an F# `.fsx` script.

Lastly, we will configure and run the targets.
The `==>` is an operator which allows us to
configure the order that the targets should run.
```fsharp
open Fake.Core.TargetOperators

"Install"
 ==> "Test"

Target.runOrDefault "Test"
```

:sweat_smile: Hopefully that makes sense! FAKE is a really
powerful tool and the above tutorial just scratches the surface.
I encourage you to read the resources below to learn more!
If you want want see a working example check out my 
[F# Utilities repo](https://github.com/ameier38/fsharp-utilities).

## Resources
- [Getting started with FAKE](https://fake.build/fake-gettingstarted.html)

Leave a comment below if you have any questions and I will try my best to answer!
