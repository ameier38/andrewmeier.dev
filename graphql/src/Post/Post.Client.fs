namespace Post

open FSharp.Data
open FSharp.UMX

type PostListProvider = JsonProvider<"resources/posts.json">

type PostProvider = JsonProvider<"resources/post.json">

type IPostClient =
    abstract member ListPosts: pageSize:int * pageToken:string option -> ListPostsResponseDto
    abstract member GetPost: postId:string -> PostDto 

type PostClient(config:AirtableConfig) =
    let get endpoint query =
        Http.RequestString(
            url = sprintf "%s/%s" config.Url endpoint,
            query = query,
            headers = [
                HttpRequestHeaders.Authorization config.ApiKey
                HttpRequestHeaders.Accept "application/json"
            ])

    interface IPostClient with
        member _.ListPosts(pageSize:int, ?offset:string) =
            let res =
                get "Post" ["pageSize", pageSize |> string; if offset.IsSome then "offset", offset.Value]
                |> PostListProvider.Parse
            let posts =
                res.Records
                |> Array.map (fun record ->
                    { PostId = record.Id
                      Title = record.Fields.Title
                      CreatedAt = record.Fields.CreatedAt
                      UpdatedAt = record.Fields.UpdatedAt })
                |> Array.toList
            let pageToken = 
                res.JsonValue.TryGetProperty("offset")
                |> Option.map (fun jval -> jval.AsString())
            { Posts = posts
              PageToken = pageToken }

        member _.GetPost(postId:string) =
            let endpoint = sprintf "Post/%s" %postId
            get endpoint []
            |> PostProvider.Parse
            |> fun doc ->
                { PostId = doc.Id
                  Title = doc.Fields.Title
                  CreatedAt = doc.Fields.CreatedAt
                  UpdatedAt = doc.Fields.UpdatedAt
                  Content = doc.Fields.Content }
            