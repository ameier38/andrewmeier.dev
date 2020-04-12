namespace Blog

open Elmish
open Fable.Core
open Fable.Core.JsInterop
open Fable.React.Helpers
open Feliz
open System

type AsyncOperation<'P,'T> =
    | Started of 'P
    | Finished of 'T

type Deferred<'T> =
    | NotStarted
    | InProgress
    | Resolved of 'T

module Cow =
    let says (msg:string) = String.Format(@"
 --------------------------
< {0} >
 --------------------------
        \   ^__^
         \  (xx)\_______
            (__)\       )\/\
             U  ||----w |
                ||     ||
", msg)


module Env =
    [<Emit("process.env[$0] ? process.env[$0] : ''")>]
    let getEnv (key:string): string = jsNative

module Log =
    let info (msg:obj) =
#if DEVEOPMENT
        Fable.Core.JS.console.info(msg)
#else
        ()
#endif

    let error (error:obj) =
#if DEVEOPMENT
        Fable.Core.JS.console.error(error)
#else
        ()
#endif

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

module Prism =
    let highlightAllUnder (ref:Browser.Types.Element): unit = importDefault "./scripts/prism.js"

module Icons =
    let githubIcon = ofImport "GitHub" "@material-ui/icons" [] []
    let twitterIcon = ofImport "Twitter" "@material-ui/icons" [] []

module Disqus =
    type DisqusConfig =
        { url: string 
          identifier: string
          title: string }
    type DisqusProps =
        | Shortname of string
        | Config of DisqusConfig
    let inline disqus (props:DisqusProps list) =
        ofImport "DiscussionEmbed" "disqus-react" (keyValueList CaseRules.LowerFirst props) []

module Error =
    let renderError (error:string) =
        Html.div [
            prop.style [
                style.display.flex
                style.justifyContent.center
            ]
            prop.children [
                Html.pre (Cow.says error)
            ]
        ]
