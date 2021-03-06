module Server.PostStore

open PostClient
open FSharp.Data
open Markdig
open Serilog
open Shared.PostStore
open Shared.Domain
open System
open System.Text.RegularExpressions

module Dto =

    module PostSummary =
        let fromRecord(record:PostProvider.Record) =
            { PostId = record.Id
              Permalink = record.Fields.Permalink
              Title = record.Fields.Title
              Summary = record.Fields.Summary
              UpdatedAt = record.Fields.UpdatedAt }

    module Post =
        let markdownPipeline = 
            MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseEmojiAndSmiley()
                .UseMathematics()
                .Build()

        let fromRecord (record:PostProvider.Record) =
            let imagePatterns =
                record.Fields.Images
                |> Array.map (fun img -> 
                    String.Format(@"!\[(.*?)\]\({0}\)", img.Filename), 
                    sprintf "![$1](%s)" img.Url)
            let replaceImages (content:string) =
                imagePatterns
                |> Array.fold (fun content (pattern, replace) ->
                    Regex.Replace(content, pattern, replace)) content
            let markdownToHtml (content:string) =
                Markdown.ToHtml(content, markdownPipeline)
            let replaceInlineCode (content:string) =
                Regex.Replace(content, "<code>", "<code class='language-none'>")
            let parsedContent =
                record.Fields.Content
                |> replaceImages
                |> markdownToHtml
                |> replaceInlineCode
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

let getPost
    (client:IPostClient) = 
    fun (req:GetPostRequest) ->
        async {
            Log.Information("Getting post {@Request}", req)
            let! record = client.GetPost(req.Permalink)
            let post = Dto.Post.fromRecord record
            return { Post = post }
        }


let listPosts
    (client:IPostClient) =
    fun (req:ListPostsRequest) ->
        async {
            try
                Log.Information("Listing posts {@Request}", req)
                let! res = client.ListPosts(req.PageSize, req.PageToken)
                let posts =
                    res.Records
                    |> Array.map Dto.PostSummary.fromRecord
                    |> Array.toList
                    |> List.sortByDescending (fun post -> post.UpdatedAt)
                let pageToken = 
                    res.JsonValue.TryGetProperty("offset")
                    |> Option.map (fun jval -> jval.AsString())
                let res =
                    { Posts = posts
                      PageToken = pageToken }
                return res
            with ex ->
                Log.Error(ex, "Error listing posts")
                return raise ex
        }

let postStore
    (client:IPostClient)
    : IPostStore =
    { getPost = getPost client
      listPosts = listPosts client }
