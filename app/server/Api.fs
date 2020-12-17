module Server.Api

open Airtable
open FSharp.Data
open Markdig
open Serilog
open Shared.Api
open Shared.Domain
open System
open System.Text.RegularExpressions

module Dto =

    module PostSummary =
        let fromRecord(record:PostProvider.Record) =
            { PostId = record.Id
              Permalink = record.Fields.Permalink
              Title = record.Fields.Title
              CreatedAt = record.Fields.CreatedAt }

    module Post =
        let fromRecord (markdownPipeline:MarkdownPipeline) (record:PostProvider.Record) =
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

let getPost
    (markdownPipeline:MarkdownPipeline)
    (client:IPostClient) = 
    fun (req:GetPostRequest) ->
        async {
            Log.Information("Getting post {@Request}", req)
            let! record = client.GetPost(req.Permalink)
            let post = record |> Dto.Post.fromRecord markdownPipeline
            return { Post = post }
        }


let listPosts
    (client:IPostClient) =
    fun (req:ListPostsRequest) ->
        async {
            Log.Information("Listing posts {@Request}", req)
            let! res = client.ListPosts(req.PageSize, req.PageToken)
            let posts =
                res.Records
                |> Array.map Dto.PostSummary.fromRecord
                |> Array.toList
                |> List.sortByDescending (fun post -> post.CreatedAt)
            let pageToken = 
                res.JsonValue.TryGetProperty("offset")
                |> Option.map (fun jval -> jval.AsString())
            let res =
                { Posts = posts
                  PageToken = pageToken }
            return res
        }

let postApi
    (markdownPipeline:MarkdownPipeline)
    (client:IPostClient)
    : IPostApi =
    { getPost = getPost markdownPipeline client
      listPosts = listPosts client }
