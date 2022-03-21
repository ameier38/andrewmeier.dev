module Server.PostClient

open Notion.Client
open Server.Config
open System
open System.Collections.Generic

type PostSummary =
    { id: string
      title: string
      summary: string
      tags: string[]
      createdAt: DateTime
      updatedAt: DateTime }
    
type PostDetail =
    { id: string
      title: string
      cover: string
      tags: string[]
      createdAt: DateTime
      updatedAt: DateTime
      content: IBlock[] }
    
type IPostClient =
    abstract List: unit -> Async<PostSummary[]>
    abstract Get: pageId:string -> Async<PostDetail option>

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

module PostSummary =
    let fromDto (page:Page) =
        let props = page.Properties
        { id = page.Id.Replace("-", "")
          title = props |> Props.getTitle "title" ""
          summary = props |> Props.getText "summary" ""
          tags = props |> Props.getMultiSelect "tags" [||]
          createdAt = props |> Props.getDate "createdAt" DateTime.UtcNow
          updatedAt = props |> Props.getDate "updatedAt" DateTime.UtcNow }
        
module PostDetail =
    let fromDto (page:Page) (blocks:IBlock[]) =
        let props = page.Properties
        { id = page.Id.Replace("-", "")
          title = props |> Props.getTitle "title" ""
          cover =
            match page.Cover with
            | :? UploadedFile as f -> f.File.Url
            | :? ExternalFile as f -> f.External.Url
            | _ -> ""
          tags = props |> Props.getMultiSelect "tags" [||]
          createdAt = props |> Props.getDate "createdAt" DateTime.UtcNow
          updatedAt = props |> Props.getDate "updatedAt" DateTime.UtcNow
          content = blocks }
        
type LivePostClient(config:Config) =
    let clientOpts = ClientOptions(AuthToken=config.NotionConfig.Token)
    let client = NotionClientFactory.Create(clientOpts)
    
    let getPublishedPage (pageId:string) = async {
        let! page = client.Pages.RetrieveAsync(pageId) |> Async.AwaitTask
        let published = page.Properties |> Props.getCheckbox "published" false
        if published then
            return Some page
        else
            return None
    }
    
    let listPublishedPages () = async {
        let mutable hasMore = true
        let mutable cursor = null
        let posts = ResizeArray()
        let filter = CheckboxFilter("published", true)
        let queryParams = DatabasesQueryParameters(StartCursor=cursor, Filter=filter)
        while hasMore do
            let! res = client.Databases.QueryAsync(config.NotionConfig.DatabaseId, queryParams) |> Async.AwaitTask
            hasMore <- res.HasMore
            cursor <- res.NextCursor
            for page in res.Results do
                posts.Add(page)
        return posts.ToArray()
    }
    
    let listBlocks (pageId:string) = async {
        let blocks = ResizeArray()
        let mutable hasMore = true
        let mutable cursor = null
        while hasMore do
            let parameters = BlocksRetrieveChildrenParameters(StartCursor=cursor)
            let! res = client.Blocks.RetrieveChildrenAsync(pageId, parameters) |> Async.AwaitTask
            hasMore <- res.HasMore
            cursor <- res.NextCursor
            blocks.AddRange(res.Results)
        return blocks.ToArray()
    }
    
    interface IPostClient with
        member _.List(): Async<PostSummary[]> = async {
            let! pages = listPublishedPages()
            return pages |> Array.map PostSummary.fromDto
        }
        
        member _.Get(pageId): Async<PostDetail option> = async {
            match! getPublishedPage pageId with
            | Some page ->
                let! blocks = listBlocks pageId
                let postDetail = PostDetail.fromDto page blocks
                return Some postDetail
            | None ->
                return None
        }

type MockPostClient() =
    interface IPostClient with
        member _.List() = async {
            return [|
               { id = "test"
                 title = "Test"
                 summary = "This is a test"
                 tags = [| "F#" |]
                 createdAt = DateTime(2022, 3, 11)
                 updatedAt = DateTime(2022, 3, 11) }
           |]
        }
        
        member _.Get(_slug:string) = async {
            let post =
                { id = ""
                  title = "Test"
                  cover = ""
                  tags = [| "F#" |]
                  createdAt = DateTime(2022, 3, 11)
                  updatedAt = DateTime(2022, 3, 11)
                  content = [||] }
            return Some post
        }
