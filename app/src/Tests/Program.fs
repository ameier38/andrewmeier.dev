open Argu
open Tests

type IntegrationTestArguments =
    | Browser_Mode of IntegrationTests.BrowserMode
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Browser_Mode _ -> "how to run the browser"
    
and Arguments =
    | [<CliPrefix(CliPrefix.None)>] All of ParseResults<IntegrationTestArguments>
    | [<CliPrefix(CliPrefix.None)>] Unit
    | [<CliPrefix(CliPrefix.None)>] Integration of ParseResults<IntegrationTestArguments>
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | All _ -> "run all the tests"
            | Unit -> "run the unit tests"
            | Integration _ -> "run the integration tests"

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<Arguments>()
    let arguments = parser.Parse(argv)
    match arguments.GetAllResults() with
    | [ All integrationTestArguments ] | [ Integration integrationTestArguments ] ->
        let browserMode =
            integrationTestArguments.TryGetResult Browser_Mode
            |> Option.defaultValue IntegrationTests.BrowserMode.Local
        IntegrationTests.run browserMode
    | [ Unit ] ->
        UnitTests.run ()
    | other -> failwith $"invalid arguments: {other}"
