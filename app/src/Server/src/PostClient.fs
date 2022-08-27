﻿module Server.PostClient

open Microsoft.Extensions.Caching.Memory
open Notion.Client
open Server.Config
open System
open System.Threading.Tasks
open System.Collections.Generic

type Post =
    { id: string
      permalink: string
      title: string
      summary: string
      icon: string
      iconAlt: string
      cover: string
      tags: string[]
      createdAt: DateTime
      updatedAt: DateTime }
    
type PostDetail =
    { post: Post
      content: IBlock[] }
    
type IPostClient =
    abstract List: unit -> Task<Post[]>
    abstract GetById: pageId:string -> Task<PostDetail option>
    abstract GetByPermalink: permalink:string -> Task<PostDetail option>

type Props = IDictionary<string,PropertyValue>

module Props =
    let tryGetValue (key:string) (props:Props) =
        match props.TryGetValue(key) with
        | true, value -> Some value
        | _ -> None
    let getTitle key defaultValue props =
        tryGetValue key props
        |> Option.bind (fun prop ->
            match prop with
            | :? TitlePropertyValue as title ->
                title.Title |> Seq.tryHead |> Option.map (fun o -> o.PlainText)
            | _ -> None)
        |> Option.defaultValue defaultValue
    let getText key defaultValue props =
        tryGetValue key props
        |> Option.bind (fun prop ->
            match prop with
            | :? RichTextPropertyValue as text ->
                text.RichText |> Seq.tryHead |> Option.map (fun o -> o.PlainText)
            | _ -> None)
        |> Option.defaultValue defaultValue
    let getCheckbox key defaultValue props =
        tryGetValue key props
        |> Option.bind (fun prop ->
            match prop with
            | :? CheckboxPropertyValue as checkbox ->
                Some checkbox.Checkbox
            | _ -> None)
        |> Option.defaultValue defaultValue
    let getDate key defaultValue props =
        tryGetValue key props
        |> Option.bind (fun prop ->
            match prop with
            | :? DatePropertyValue as date ->
                if isNull date.Date then None
                else Option.ofNullable date.Date.Start
            | _ -> None)
        |> Option.defaultValue defaultValue
    let getSelect key defaultValue props =
        tryGetValue key props
        |> Option.bind (fun prop ->
            match prop with
            | :? SelectPropertyValue as select -> Some select.Select.Name
            | _ -> None)
        |> Option.defaultValue defaultValue
    let getMultiSelect key defaultValue props =
        tryGetValue key props
        |> Option.map (fun prop ->
            match prop with
            | :? MultiSelectPropertyValue as multiSelect ->
                multiSelect.MultiSelect
                |> Seq.map (fun s -> s.Name)
                |> Seq.toArray
            | _ -> Array.empty)
        |> Option.defaultValue defaultValue
        
module File =
    let getIconUrl (icon:IPageIcon) =
        match icon with
        | :? UploadedFile as f -> f.File.Url
        | :? ExternalFile as f -> f.External.Url
        | _ -> ""
    let getUrl (file:FileObject) =
        match file with
        | :? UploadedFile as f -> f.File.Url
        | :? ExternalFile as f -> f.External.Url
        | _ -> ""

module Post =
    let fromDto (page:Page) =
        let props = page.Properties
        { id = page.Id.Replace("-", "")
          permalink = props |> Props.getText "permalink" ""
          title = props |> Props.getTitle "title" ""
          summary = props |> Props.getText "summary" ""
          icon = File.getIconUrl page.Icon
          iconAlt = props |> Props.getText "iconAlt" ""
          cover = File.getUrl page.Cover
          tags = props |> Props.getMultiSelect "tags" [||]
          createdAt = props |> Props.getDate "createdAt" DateTime.UtcNow
          updatedAt = props |> Props.getDate "updatedAt" DateTime.UtcNow }
        
type LivePostClient(config:NotionConfig, cache:IMemoryCache) =
    let clientOpts = ClientOptions(AuthToken=config.Token)
    let client = NotionClientFactory.Create(clientOpts)
    
    // Get the page id from the permalink and cache the result
    let getPageId(permalink:string) = task {
        match cache.TryGetValue(permalink) with
        | true, value ->
            let pageId = unbox<string> value
            return Some pageId
        | _ ->
            let filter = RichTextFilter("permalink", permalink)
            let queryParams = DatabasesQueryParameters(Filter=filter)
            let! res = client.Databases.QueryAsync(config.DatabaseId, queryParams)
            match Seq.tryHead res.Results with
            | Some page ->
                // The cache entry will take up 1/1000 entries.
                // The entries limit is defined in Program.fs when adding the cache.
                let cacheEntryOpts = MemoryCacheEntryOptions(Size=1L)
                let pageId = cache.Set(permalink, page.Id, cacheEntryOpts)
                return Some pageId
            | None ->
                return None
    }
    
    let getPublishedPage (pageId:string) = task {
        let! page = client.Pages.RetrieveAsync(pageId)
        let status = page.Properties |> Props.getSelect "status" ""
        if status = "Published" then
            return Some page
        else
            return None
    }
    
    let listPublishedPages () = task {
        let mutable hasMore = true
        let mutable cursor = null
        let posts = ResizeArray()
        // Filter for pages where the 'status' select field is 'Published'
        let filter = SelectFilter("status", "Published")
        let queryParams = DatabasesQueryParameters(StartCursor=cursor, Filter=filter)
        while hasMore do
            let! res = client.Databases.QueryAsync(config.DatabaseId, queryParams)
            hasMore <- res.HasMore
            cursor <- res.NextCursor
            for page in res.Results do
                posts.Add(page)
        return
            posts
            |> Seq.map Post.fromDto
            |> Seq.sortByDescending (fun p -> p.createdAt)
            |> Seq.toArray
    }
    
    let rec listBlocks (blockId:string): Task<IBlock[]> = task {
        let blocks = ResizeArray<IBlock>()
        let mutable hasMore = true
        let mutable cursor = null
        while hasMore do
            let parameters = BlocksRetrieveChildrenParameters(StartCursor=cursor)
            let! res = client.Blocks.RetrieveChildrenAsync(blockId, parameters)
            hasMore <- res.HasMore
            cursor <- res.NextCursor
            blocks.AddRange(res.Results)
        return blocks.ToArray()
    }
    
    let getPostById (pageId:string) = task {
        match! getPublishedPage pageId with
        | Some page ->
            let post = Post.fromDto page
            let! blocks = listBlocks pageId
            let postDetail = { post = post; content = blocks  }
            return Some postDetail
        | None ->
            return None
    }
    
    let getPostByPermalink (permalink:string) = task {
        match! getPageId permalink with
        | Some pageId ->
            return! getPostById pageId
        | None ->
            return None
    }
    
    interface IPostClient with
        member _.List() = listPublishedPages()
        
        member _.GetById(pageId) = getPostById pageId
        
        member _.GetByPermalink(permalink) = getPostByPermalink(permalink)

type MockPostClient() =
    let post1 = 
       { id = "4d7ac503a7a64cc0ab757df70c7c7f7b"
         permalink = "test"
         title = "Test"
         summary = "This is a test"
         icon = ""
         iconAlt = ""
         cover = ""
         tags = [| "F#" |]
         createdAt = DateTime(2022, 3, 11)
         updatedAt = DateTime(2022, 3, 11) }
    let post2 = 
       { id = "33f11309306447ce8a48e962f0e0d814"
         permalink = "another-test"
         title = "Another Test"
         summary = "This is another test"
         icon = ""
         iconAlt = ""
         cover = ""
         tags = [| "F#" |]
         createdAt = DateTime(2022, 3, 11)
         updatedAt = DateTime(2022, 3, 11) }
    let lookupById = Map.ofList [
        post1.id, post1
        post2.id, post2
    ]
    let lookupByPermalink = Map.ofList [
        post1.permalink, post1
        post2.permalink, post2
    ]
    interface IPostClient with
        member _.List() = task {
            return [| post1; post2 |]
        }
        
        member _.GetById(pageId:string) = task {
            match lookupById |> Map.tryFind pageId with
            | Some post ->
                return Some { post = post; content = [||] }
            | None ->
                return None
        }
        
        member _.GetByPermalink(permalink:string) = task {
            match lookupByPermalink |> Map.tryFind permalink with
            | Some post ->
                return Some { post = post; content = [||] }
            | None ->
                return None
        }
