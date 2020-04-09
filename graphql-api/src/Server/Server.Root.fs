module Server.Root

open FSharp.Data.GraphQL.Types

type Root = { _empty: bool option }

let Query
    (postClient:Post.IPostClient) = 
    Define.Object<Root>(
        name = "Query",
        fields = [
            Post.Fields.listPostsField postClient
            Post.Fields.getPostField postClient
        ])
