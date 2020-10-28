module Blog.Graphql

open Fable.SimpleHttp
open Fable.SimpleJson
open System

let listPostsQuery = """
query ListPosts($input:ListPostsInput!) {
    listPosts(input: $input) {
        posts {
            postId
            permalink
            title
            createdAt
        }
    }
}
"""

let getPostQuery = """
query GetPost($input:GetPostInput!) {
    getPost(input: $input) {
        postId
        permalink
        title
        cover
        createdAt
        updatedAt
        content
    }
}
"""

type PostSummaryDto =
    { postId: string
      permalink: string
      title: string
      createdAt: DateTimeOffset }

type PostDto =
    { postId: string
      permalink: string
      title: string
      cover: string
      createdAt: DateTimeOffset
      updatedAt: DateTimeOffset
      content: string }

type GetPostInputDto =
    { permalink: string }

type ListPostsInputDto =
    { pageSize: int option
      pageToken: string option }

type ListPostsResponseDto =
    { posts: PostSummaryDto list
      pageToken: string option }

type Data =
    { listPosts: ListPostsResponseDto option
      getPost: PostDto option }

type Response =
    { data: Data
      errors: (string list) option }

type IGraphqlClient =
    abstract member ListPosts: unit -> Async<Result<ListPostsResponseDto,string>>
    abstract member GetPost: permalink:string -> Async<Result<PostDto,string>>

type GraphqlClient() =
    let scheme = Env.getEnv "GRAPHQL_SCHEME"
    let host = Env.getEnv "GRAPHQL_HOST"
    let port = Env.getEnv "GRAPHQL_PORT"
    let url =
        match port with
        | "" | "80" -> sprintf "%s://%s" scheme host
        | port -> sprintf "%s://%s:%s" scheme host port
    interface IGraphqlClient with
        member _.ListPosts() =
            async {
                let input =
                    { pageSize = Some 50
                      pageToken = None }
                let requestData =
                    {| query = listPostsQuery
                       variables = {| input = input |} |} 
                    |> Json.stringify
                let! (statusCode, responseData) = Http.post url requestData
                return
                    match statusCode with
                    | 200 -> 
                        let parsedResponse =
                            responseData
                            |> Json.parseAs<Response>
                        match parsedResponse.data.listPosts with
                        | Some posts -> Ok posts
                        | None -> Error "failed to parse"
                    | other -> 
                        let response =
                            responseData
                            |> Json.parseAs<Response>
                        let error = sprintf "Error %i: %A" other response.errors
                        Log.error(error)
                        Error error
            }
        member _.GetPost(permalink:string) =
            async {
                let input = { permalink = permalink }
                let requestData =
                    {| query = getPostQuery
                       variables = {| input = input |} |}
                    |> Json.stringify
                let! (statusCode, responseData) = Http.post url requestData
                return
                    match statusCode with
                    | 200 ->
                        let response =
                            responseData
                            |> Json.parseAs<Response>
                        match response.data.getPost with
                        | Some post -> Ok post
                        | None -> Error "failed to parse"
                    | other ->
                        let response =
                            responseData
                            |> Json.parseAs<Response>
                        let error = sprintf "Error %i: %A" other response.errors
                        Log.error(error)
                        Error error
            }
