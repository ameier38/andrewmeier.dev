module Client.Program

open Browser.Dom
open Feliz

#if DEBUG
open Fable.Core.JsInterop
importSideEffects "../scripts/hmr.mjs"
#endif

ReactDOM.render(App.App(), document.getElementById "app")
