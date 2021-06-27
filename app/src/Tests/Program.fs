open Argu
open Tests

type IntegrationTestArguments =
    | Browser_Mode of IntegrationTests.BrowserMode
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Browser_Mode _ -> "how to run the browser"
    
and Arguments =
    | [<CliPrefix(CliPrefix.None)>] Test_Units
    | [<CliPrefix(CliPrefix.None)>] Test_Integrations of ParseResults<IntegrationTestArguments>
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Test_Units -> "run the unit tests"
            | Test_Integrations _ -> "run the integration tests"

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<Arguments>()
    let arguments = parser.Parse(argv)
    match arguments.GetAllResults() with
    | [ Test_Integrations integrationTestArguments ] ->
        let browserMode =
            integrationTestArguments.TryGetResult Browser_Mode
            |> Option.defaultValue IntegrationTests.BrowserMode.Local
        IntegrationTests.run browserMode
    | [ Test_Units ] ->
        UnitTests.run ()
    | other -> failwith $"invalid arguments: {other}"
