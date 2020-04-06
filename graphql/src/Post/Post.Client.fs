namespace Post

open FSharp.Data
open FSharp.UMX

type PostListProvider = JsonProvider<Airtable.ListPostsResponse>

type PostProvider = JsonProvider<Airtable.GetPostResponse>

type IPostClient =
    abstract member ListPosts: pageSize:int * pageToken:string option -> ListPostsResponseDto
    abstract member GetPost: postId:string -> PostDto 

type PostClient(config:AirtableConfig) =
    let get endpoint query =
        let auth = sprintf "Bearer %s" config.ApiKey
        Http.RequestString(
            url = sprintf "%s/%s" config.Url endpoint,
            query = query,
            headers = [
                HttpRequestHeaders.Authorization auth
                HttpRequestHeaders.Accept HttpContentTypes.Json
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
                      UpdatedAt = record.Fields.UpdatedAt
                      Content = record.Fields.Content })
                |> Array.toList
            let pageToken = 
                res.JsonValue.TryGetProperty("offset")
                |> Option.map (fun jval -> jval.AsString())
            { Posts = posts
              PageToken = pageToken }

        member _.GetPost(postId:string) =
            let endpoint = sprintf "Post/%s" %postId
            let res = 
                get endpoint []
                |> PostProvider.Parse
            { PostId = res.Id
              Title = res.Fields.Title
              CreatedAt = res.Fields.CreatedAt
              UpdatedAt = res.Fields.UpdatedAt
              Content = res.Fields.Content }
            
type MockPostClient() =
    interface IPostClient with
        member _.ListPosts(_pageSize:int, ?_offset:string) =
            let res = PostListProvider.GetSample()
            let posts =
                res.Records
                |> Array.map (fun record ->
                    { PostId = record.Id
                      Title = record.Fields.Title
                      CreatedAt = record.Fields.CreatedAt
                      UpdatedAt = record.Fields.UpdatedAt
                      Content = record.Fields.Content })
                |> Array.toList
            { Posts = posts
              PageToken = None }

        member _.GetPost(_postId:string) = 
            let res = PostProvider.GetSample()
            { PostId = res.Id
              Title = res.Fields.Title
              CreatedAt = res.Fields.CreatedAt
              UpdatedAt = res.Fields.UpdatedAt
              Content = res.Fields.Content }
