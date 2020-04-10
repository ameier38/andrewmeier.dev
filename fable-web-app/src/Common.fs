namespace Blog

open Fable.Core
open Elmish

module Env =

    [<Emit("process.env[$0] ? process.env[$0] : ''")>]
    let getEnv (key:string): string = jsNative

module Cmd =
    let fromAsync (work:Async<'M>): Cmd<'M> =
        let asyncCmd (dispatch:'M -> unit): unit =
            let asyncDispatch =
                async {
                    let! msg = work
                    dispatch msg
                }
            Async.StartImmediate asyncDispatch
        Cmd.ofSub asyncCmd

