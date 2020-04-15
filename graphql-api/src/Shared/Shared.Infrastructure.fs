namespace Shared

open System
open System.IO
open System.Text.RegularExpressions

module Env =

    let getEnv key defaultValueOpt =
        match Environment.GetEnvironmentVariable(key) with
        | s when String.IsNullOrEmpty(s) ->
            match defaultValueOpt with
            | Some value -> value
            | None -> failwithf "%s not found and no default provided" key
        | s -> s

    let getSecret secretName secretKey defaultValueOpt =
        let secretsDir = Some "/var/secrets" |> getEnv "SECRETS_DIR"
        let secretPath = Path.Combine(secretsDir, secretName, secretKey)
        if File.Exists(secretPath) then
            File.ReadAllText(secretPath).Trim()
        else
            match defaultValueOpt with
            | Some value -> value
            | None -> failwithf "%s not found and no default provided" secretPath

module Regex =
    let (|Regex|_|) (pattern:string) (s:string) =
        let m = Regex.Match(s, pattern)
        if m.Success then Some(List.tail [for g in m.Groups -> g.Value])
        else None
