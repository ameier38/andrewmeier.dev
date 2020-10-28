module Blog.Main

open Elmish
open Elmish.React

#if DEVELOPMENT
open Elmish.HMR
#endif

Program.mkProgram App.init App.update App.render
|> Program.withReactSynchronous "app"
|> Program.run