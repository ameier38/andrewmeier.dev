namespace Shared

open System
open System.IO
open System.Text.RegularExpressions

module Env =

    let getEnv key defaultValue =
        match Environment.GetEnvironmentVariable(key) with
        | s when String.IsNullOrEmpty(s) -> defaultValue
        | s -> s

    let getSecret secretName secretKey defaultEnv defaultValue =
        let secretsDir = getEnv "SECRETS_DIR" "/var/secrets" 
        let secretPath = Path.Combine(secretsDir, secretName, secretKey)
        if File.Exists(secretPath) then
            File.ReadAllText(secretPath).Trim()
        else
            getEnv defaultEnv defaultValue

module Regex =
    let (|Regex|_|) (pattern:string) (s:string) =
        let m = Regex.Match(s, pattern)
        if m.Success then Some(List.tail [for g in m.Groups -> g.Value])
        else None
