[<RequireQualifiedAccess>]
module Config

open Fable.Core

[<Emit("process.env[$0] ? process.env[$0] : ''")>]
let variable (key:string): string = jsNative
