module Server.Root

open FSharp.Data.GraphQL.Types
open Server.Post.Client

type Root = { _empty: bool option }

let Query
    (postClient:IPostClient) = 
    Define.Object<Root>(
        name = "Query",
        fields = [
            Post.Fields.listPostsField postClient
            Post.Fields.getPostField postClient
        ])
