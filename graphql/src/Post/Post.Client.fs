namespace Post

open FSharp.Data
open FSharp.UMX
open Serilog
open System
open System.Text.RegularExpressions

type PostListProvider = JsonProvider<Airtable.ListPostsResponse>

type PostProvider = JsonProvider<Airtable.GetPostResponse>

type IPostClient =
    abstract member ListPosts: pageSize:int * pageToken:string option -> ListPostsResponseDto
    abstract member GetPost: postId:string -> PostDto 

module Dto =

    module PostSummaryDto =
        let fromRecord(record:PostListProvider.Record) =
            { PostId = record.Id
              Title = record.Fields.Title
              CreatedAt = record.Fields.CreatedAt }

    module PostDto =
        let fromRecord (record:PostProvider.Root) =
            let imagePatterns =
                record.Fields.Images
                |> Array.map (fun img -> 
                    String.Format(@"!\[(.*?)\]\({0}\)", img.Filename), 
                    sprintf "![$1](%s)" img.Thumbnails.Large.Url)
            let parsedContent =
                imagePatterns
                |> Array.fold (fun content (pattern, replace) ->
                    Regex.Replace(content, pattern, replace)
                ) record.Fields.Content
            let coverPattern = @"^cover.(png|gif|jpg)$"
            let cover =
                record.Fields.Images
                |> Array.tryFind (fun img -> Regex.IsMatch(img.Filename, coverPattern)) 
                |> Option.map (fun img -> img.Thumbnails.Large.Url)
                |> Option.defaultValue ""
            { PostId = record.Id
              Title = record.Fields.Title
              Cover = cover
              CreatedAt = record.Fields.CreatedAt
              UpdatedAt = record.Fields.UpdatedAt
              Content = parsedContent }

type PostClient(config:AirtableConfig) =
    let get endpoint query =
        let auth = sprintf "Bearer %s" config.ApiKey
        let filter = System.Web.HttpUtility.UrlEncode("({publish})")
        Http.RequestString(
            url = sprintf "%s/%s?filterByFormula=%s" config.Url endpoint filter,
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
                |> Array.map Dto.PostSummaryDto.fromRecord
                |> Array.toList
                |> List.sortBy (fun post -> post.CreatedAt)
            let pageToken = 
                res.JsonValue.TryGetProperty("offset")
                |> Option.map (fun jval -> jval.AsString())
            { Posts = posts
              PageToken = pageToken }

        member _.GetPost(postId:string) =
            let endpoint = sprintf "Post/%s" %postId
            get endpoint []
            |> PostProvider.Parse
            |> Dto.PostDto.fromRecord
            
type MockPostClient() =
    interface IPostClient with
        member _.ListPosts(_pageSize:int, ?_offset:string) =
            let res = PostListProvider.GetSample()
            let posts =
                res.Records
                |> Array.map Dto.PostSummaryDto.fromRecord
                |> Array.toList
            { Posts = posts
              PageToken = None }

        member _.GetPost(_postId:string) = 
            PostProvider.GetSample()
            |> Dto.PostDto.fromRecord
