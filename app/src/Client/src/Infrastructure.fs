namespace Client

open Fable.Core

type Deferred<'T> =
    | HasNotStarted
    | InProgress
    | Resolved of 'T
    | Error of string

module Env =
    [<Emit("import.meta.env[$0] ? import.meta.env[$0] : $1")>]
    let getEnv (key:string) (defaultValue:string): string = jsNative

module Prism =
    [<Emit("window.Prism.highlightAllUnder($0)")>]
    let highlightAllUnder (el:Browser.Types.Element): unit = jsNative
    