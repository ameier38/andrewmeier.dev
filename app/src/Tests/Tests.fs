module Tests

open canopy.classic
open canopy.types

open System
open Xunit

type Fixture() =
    do
        start ChromeHeadless
    interface IDisposable with
        member _.Dispose() =
            quit()

type Tests() =
    interface IClassFixture<Fixture>
    
    [<Fact>]
    member _.``Navigating to home and clicking post should work`` () =
        url "http://localhost:5000"
        describe "checking post summary title"
        "#test h3" == "Test"
        describe "navigating to post"
        click "#test"
        on "http://localhost:5000/test"
        describe "checking post title"
        "h1" == "Test"
