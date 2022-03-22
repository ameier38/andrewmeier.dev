module Tests

open canopy.classic
open canopy.types

open System
open Xunit

module Env =
    let variable (key:string) (defaultValue:string) =
        match Environment.GetEnvironmentVariable(key) with
        | s when String.IsNullOrEmpty(s) -> defaultValue
        | s -> s

type Fixture() =
    do
        let isCI = Env.variable "CI" "false" |> Boolean.Parse
        if isCI then
            let defaultChromeDir = AppContext.BaseDirectory
            // ref: https://github.com/actions/virtual-environments/blob/main/images/linux/Ubuntu2004-Readme.md
            let chromeDir = Env.variable "CHROMEWEBDRIVER" defaultChromeDir
            canopy.configuration.chromeDir <- chromeDir
        start ChromeHeadless
    interface IDisposable with
        member _.Dispose() =
            quit()

type Tests() =
    interface IClassFixture<Fixture>
    
    [<Fact>]
    member _.``Navigating to home shows list of posts`` () =
        url "http://localhost:5000"
        describe "there should be two posts"
        count ".post" 2
        "#test h3" == "Test"
        
    [<Fact>]
    member _.``Clicking post should navigate to detail`` () =
        url "http://localhost:5000"
        describe "navigating to post"
        click "#test"
        on "http://localhost:5000/test"
        describe "post title should be 'Test'"
        "h1" == "Test"
        
    [<Fact>]
    member _.``Permalink url should navigate to post`` () =
        url "http://localhost:5000/another-test"
        on "http://localhost:5000/another-test"
        describe "post title should be 'Another Test'"
        "h1" == "Another Test"
        
    [<Fact>]
    member _.``Non existent post should show not found`` () =
        url "http://localhost:5000/blah"
        describe "should be on not found page"
        "h1" == "Page not found"
