namespace global

open System
open System.IO

module Env =

    let variable (key:string) (defaultValue:string) =
        match Environment.GetEnvironmentVariable(key) with
        | s when String.IsNullOrEmpty(s) -> defaultValue
        | s -> s

    let secret secretName secretKey defaultEnv defaultValue =
        let secretsDir = variable "SECRETS_DIR" "/var/secrets" 
        let secretPath = Path.Combine(secretsDir, secretName, secretKey)
        if File.Exists(secretPath) then
            File.ReadAllText(secretPath).Trim()
        else
            variable defaultEnv defaultValue
