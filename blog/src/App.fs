module App

open Elmish
open Elmish.React
open Feliz

let messages =
    [| "Hello"
       "World"
       "My"
       "Old"
       "Friend" |]

let cnt = messages |> Array.length

type State =
    { Index: int
      Message: string }

type Message =
    | Previous
    | Next

let init () =
    { Index = 0
      Message = "Home" }

let update (msg:Message) (state:State): State =
    match msg with
    | Next ->
        let newIdx = state.Index + 1 |> min (cnt - 1)
        let newMsg = messages.[newIdx]
        { Index = newIdx
          Message = newMsg }
    | Previous ->
        let newIdx = state.Index - 1 |> max 0
        let newMsg = messages.[newIdx]
        { Index = newIdx
          Message = newMsg }

let render (state:State) (dispatch:Message -> unit) =
    Html.div [
        Html.h1 state.Message
        Html.button [
            prop.onClick (fun _ -> dispatch Previous)
            prop.text "Previous"
        ]
        Html.button [
            prop.onClick (fun _ -> dispatch Next)
            prop.text "Next"
        ]
    ]

Program.mkSimple init update render
|> Program.withReactSynchronous "app"
|> Program.run
