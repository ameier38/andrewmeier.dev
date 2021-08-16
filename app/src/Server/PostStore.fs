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
            { postId = record.Id
              permalink = record.Fields.Permalink
              title = record.Fields.Title
              summary = record.Fields.Summary
              updatedAt = record.Fields.UpdatedAt }

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
                    $"![$1]({img.Url})")
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
            { postId = record.Id
              permalink = record.Fields.Permalink
              title = record.Fields.Title
              cover = cover
              createdAt = record.Fields.CreatedAt
              updatedAt = record.Fields.UpdatedAt
              content = parsedContent }

let getPost
    (client:IPostClient) = 
    fun (req:GetPostRequest) ->
        async {
            Log.Information("Getting post {@Request}", req)
            let! record = client.GetPost(req.permalink)
            let post = Dto.Post.fromRecord record
            return { post = post }
        }


let listPosts
    (client:IPostClient) =
    fun (req:ListPostsRequest) ->
        async {
            try
                Log.Information("Listing posts {@Request}", req)
                let! res = client.ListPosts(req.pageSize, req.pageToken)
                let posts =
                    res.Records
                    |> Array.map Dto.PostSummary.fromRecord
                    |> Array.toList
                    |> List.sortByDescending (fun post -> post.updatedAt)
                let pageToken = 
                    res.JsonValue.TryGetProperty("offset")
                    |> Option.map (fun jval -> jval.AsString())
                let res =
                    { posts = posts
                      pageToken = pageToken }
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
