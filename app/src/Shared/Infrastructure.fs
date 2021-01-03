namespace Shared

open System
open System.IO

#if !FABLE_COMPILER
module Env =

    let getEnv (key:string) (defaultValue:string) =
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
#endif
