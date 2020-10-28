module Server.Post.Client

open Server
open Server.Post
open Server.Post.Dto

open FSharp.Data
open Markdig
open System
open System.Text.RegularExpressions

type PostListProvider = JsonProvider<Airtable.ListPostsResponse>

type IPostClient =
    abstract member ListPosts: pageSize:int * pageToken:string option -> ListPostsResponseDto
    abstract member GetPost: permalink:string -> PostDto 

module Dto =

    module PostSummaryDto =
        let fromRecord(record:PostListProvider.Record) =
            { PostId = record.Id
              Permalink = record.Fields.Permalink
              Title = record.Fields.Title
              CreatedAt = record.Fields.CreatedAt }

    module PostDto =
        let fromRecord (markdownPipeline:MarkdownPipeline) (record:PostListProvider.Record) =
            let imagePatterns =
                record.Fields.Images
                |> Array.map (fun img -> 
                    String.Format(@"!\[(.*?)\]\({0}\)", img.Filename), 
                    sprintf "![$1](%s)" img.Url)
            let replaceImages (content:string) (pattern:string, replace:string) =
                Regex.Replace(content, pattern, replace)
            let parsedContent =
                imagePatterns
                |> Array.fold replaceImages record.Fields.Content
                |> fun content -> Markdown.ToHtml(content, markdownPipeline)
            let coverPattern = @"^cover.(png|gif|jpg)$"
            let cover =
                record.Fields.Images
                |> Array.tryFind (fun img -> Regex.IsMatch(img.Filename, coverPattern)) 
                |> Option.map (fun img -> img.Url)
                |> Option.defaultValue ""
            { PostId = record.Id
              Permalink = record.Fields.Permalink
              Title = record.Fields.Title
              Cover = cover
              CreatedAt = record.Fields.CreatedAt
              UpdatedAt = record.Fields.UpdatedAt
              Content = parsedContent }

type PostClient(config:AirtableConfig) =
    let markdownPipeline = 
        MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseEmojiAndSmiley()
            .UseMathematics()
            .Build()

    let get endpoint query =
        let auth = sprintf "Bearer %s" config.ApiKey
        Http.RequestString(
            url = sprintf "%s/%s/%s" config.ApiUrl config.BaseId endpoint,
            query = query,
            headers = [
                HttpRequestHeaders.Authorization auth
                HttpRequestHeaders.Accept HttpContentTypes.Json
            ])

    interface IPostClient with
        member _.ListPosts(pageSize:int, ?offset:string) =
            let formula = "AND({status} = 'Published', {permalink} != 'about')"
            let query =
                [ "pageSize", pageSize |> string
                  "filterByFormula", formula
                  for field in ["permalink"; "title"; "created_at"] do
                    "fields[]", field
                  if offset.IsSome then 
                    "offset", offset.Value ]
            let res =
                get "Post" query
                |> PostListProvider.Parse
            let posts =
                res.Records
                |> Array.map Dto.PostSummaryDto.fromRecord
                |> Array.toList
                |> List.sortByDescending (fun post -> post.CreatedAt)
            let pageToken = 
                res.JsonValue.TryGetProperty("offset")
                |> Option.map (fun jval -> jval.AsString())
            { Posts = posts
              PageToken = pageToken }

        member _.GetPost(permalink:string) =
            let formula = sprintf "AND({status} = 'Published', {permalink} = '%s')" permalink
            let query = [ "filterByFormula", formula ]
            let res =
                get "Post" query
                |> PostListProvider.Parse
            match res.Records with
            | [||] ->
                failwithf "%s not found" permalink
            | [| record |] ->
                record
                |> Dto.PostDto.fromRecord markdownPipeline
            | _ ->
                failwithf "found multiple posts with the same permalink: %s" permalink

type MockPostClient() =
    let markdownPipeline = 
        MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseEmojiAndSmiley()
            .UseMathematics()
            .Build()

    interface IPostClient with
        member _.ListPosts(_pageSize:int, ?_offset:string) =
            let res = PostListProvider.GetSample()
            let posts =
                res.Records
                |> Array.filter (fun record -> 
                    record.Fields.Status = "Published"
                    && record.Fields.Permalink <> "about")
                |> Array.map Dto.PostSummaryDto.fromRecord
                |> Array.toList
            { Posts = posts
              PageToken = None }

        member _.GetPost(permalink:string) = 
            let res = PostListProvider.GetSample()
            res.Records
            |> Array.filter (fun record ->
                record.Fields.Status = "Published"
                && record.Fields.Permalink = permalink) 
            |> Array.head
            |> Dto.PostDto.fromRecord markdownPipeline
