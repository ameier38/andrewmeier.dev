module Server.Types

open Server.ViewEngine

type Response =
    { page:HtmlElement
      meta:HtmlElement seq
      push:string option }
    
module Response =
    let create (page:HtmlElement) =
        { page = page
          meta = Seq.empty
          push = None }
    let withMeta (meta:HtmlElement seq) (response:Response) =
        { response with meta = meta }
    let withPush (push:string) (response:Response) =
        { response with push = Some push }
    
