namespace Client

open Fable.Core
open Fable.Core.JsInterop
open Fable.React.Helpers

type Deferred<'T> =
    | HasNotStarted
    | InProgress
    | Resolved of 'T
    | Error of string

module Env =
    [<Emit("process.env[$0] ? process.env[$0] : $1")>]
    let getEnv (key:string) (defaultValue:string): string = jsNative

module Icons =
    let githubIcon = ofImport "GitHub" "@material-ui/icons" [] []
    let twitterIcon = ofImport "Twitter" "@material-ui/icons" [] []

module Prism =
    type IPrism =
        abstract member highlightAllUnder: Browser.Types.Element -> unit

    let Prism:IPrism = importDefault "prismjs"

    let highlightAllUnder = Prism.highlightAllUnder

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
