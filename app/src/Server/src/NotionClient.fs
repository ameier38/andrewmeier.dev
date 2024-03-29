﻿module Server.NotionClient

open Microsoft.Extensions.Caching.Memory
open Notion.Client
open Server.Config
open Serilog
open System
open System.Threading.Tasks
open System.Collections.Generic

type PageProperties =
    { id:string
      permalink:string
      title:string
      summary:string
      icon:string
      iconAlt:string
      cover:string
      tags:string[]
      createdAt:DateTimeOffset
      updatedAt:DateTimeOffset }
    
type PageDetail =
    { properties:PageProperties
      content:IBlock[] }
    
type INotionClient =
    abstract List: unit -> Task<PageProperties[]>
    abstract GetById: permalink:string -> Task<PageDetail option>
    abstract GetByPermalink: permalink:string -> Task<PageDetail option>

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
                else date.Date.Start |> Option.ofNullable |> Option.map DateTimeOffset
            | _ -> None)
        |> Option.defaultValue defaultValue
    let getStatus key defaultValue props =
        tryGetValue key props
        |> Option.bind (fun prop ->
            match prop with
            | :? StatusPropertyValue as status -> Some status.Status.Name
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

module PageProperties =
    let fromDto (dto:Page) : PageProperties =
        let props = dto.Properties
        { id = dto.Id.Replace("-", "")
          permalink = props |> Props.getText "permalink" ""
          title = props |> Props.getTitle "title" ""
          summary = props |> Props.getText "summary" ""
          icon = File.getIconUrl dto.Icon
          iconAlt = props |> Props.getText "iconAlt" ""
          cover = File.getUrl dto.Cover
          tags = props |> Props.getMultiSelect "tags" [||]
          createdAt = props |> Props.getDate "createdAt" DateTimeOffset.UtcNow
          updatedAt = props |> Props.getDate "updatedAt" DateTimeOffset.UtcNow }
        
type LiveNotionClient(appEnv:AppEnv, config:NotionConfig, cache:IMemoryCache) =
    let clientOpts = ClientOptions(AuthToken=config.Token)
    let client = NotionClientFactory.Create(clientOpts)
    
    let getPageId (permalink:string) = task {
        let cacheKey = $"permalink:{permalink}"
        match cache.TryGetValue cacheKey with
        | true, value ->
            Log.Debug("Cache hit: {Key}", cacheKey)
            let pageId = unbox<string> value
            return Some pageId
        | false, _ ->
            Log.Debug("Cache miss: {Key}", cacheKey)
            let filter = RichTextFilter("permalink", permalink)
            let queryParams = DatabasesQueryParameters(Filter=filter)
            let! res = client.Databases.QueryAsync(config.DatabaseId, queryParams)
            match Seq.tryHead res.Results with
            | Some page ->
                let cacheEntryOpts = MemoryCacheEntryOptions(Size=1L)
                let pageId = cache.Set(cacheKey, page.Id, cacheEntryOpts)
                return Some pageId
            | None ->
                return None
    }
    
    let getPublishedPage (pageId:string) = task {
        let! page = client.Pages.RetrieveAsync(pageId)
        let status = page.Properties |> Props.getStatus "Status" ""
        let page =
            match appEnv, status with
            | AppEnv.Prod, "Published" -> Some page
            | AppEnv.Dev, ("Published" | "In progress") -> Some page
            | _ -> None
        return page
    }
    
    let listPublishedPages () = task {
        let mutable hasMore = true
        let mutable cursor = null
        let pages = ResizeArray()
        let filter =
            let publishedFilter = StatusFilter("Status", "Published") :> Filter
            match appEnv with
            | AppEnv.Prod -> publishedFilter
            | AppEnv.Dev ->
                let inProgressFilter = StatusFilter("Status", "In progress") :> Filter
                CompoundFilter(``or``=List([publishedFilter; inProgressFilter])) :> Filter
        let queryParams = DatabasesQueryParameters(StartCursor=cursor, Filter=filter)
        while hasMore do
            let! res = client.Databases.QueryAsync(config.DatabaseId, queryParams)
            hasMore <- res.HasMore
            cursor <- res.NextCursor
            for page in res.Results do pages.Add(page)
        return pages
    }
    
    let rec listBlocks (blockId:string): Task<IBlock[]> = task {
        let blocks = ResizeArray<IBlock>()
        let mutable hasMore = true
        let mutable cursor = null
        while hasMore do
            let parameters = BlocksRetrieveChildrenParameters(StartCursor=cursor)
            let! res = client.Blocks.RetrieveChildrenAsync(blockId, parameters)
            for block in res.Results do
                if block.HasChildren then
                    let! children = listBlocks block.Id
                    match block with
                    | :? BulletedListItemBlock as b ->
                        let convertedChildren = children |> Seq.map (fun c -> c :?> INonColumnBlock)
                        b.BulletedListItem.Children <- convertedChildren
                    | :? NumberedListItemBlock as b ->
                        let convertedChildren = children |> Seq.map (fun c -> c :?> INonColumnBlock)
                        b.NumberedListItem.Children <- convertedChildren
                    | other -> Log.Warning("Block {Block} children not supported yet", other)
                else
                    match block with
                    | :? BulletedListItemBlock as b -> b.BulletedListItem.Children <- Seq.empty
                    | :? NumberedListItemBlock as b -> b.NumberedListItem.Children <- Seq.empty
                    | other -> Log.Warning("Block {Block} children not supported yet", other)
            hasMore <- res.HasMore
            cursor <- res.NextCursor
            blocks.AddRange(res.Results)
        return blocks.ToArray()
    }
    
    let listPages () = task {
        let! pages = listPublishedPages ()
        return
            pages
            |> Seq.map PageProperties.fromDto
            |> Seq.sortByDescending (fun p -> p.createdAt)
            |> Seq.toArray
    }
    
    let getPageById (pageId:string) = task {
        match! getPublishedPage pageId with
        | Some page ->
            let properties = PageProperties.fromDto page
            let! blocks = listBlocks pageId
            let detail = { properties = properties; content = blocks  }
            return Some detail
        | None ->
            return None
    }
    
    let getPageByPermalink (permalink:string) = task {
        match! getPageId permalink with
        | Some pageId ->
            return! getPageById pageId
        | None ->
            return None
    }
    
    interface INotionClient with
        member _.List() = listPages ()
        member _.GetById(pageId) = getPageById pageId
        member _.GetByPermalink(permalink) = getPageByPermalink permalink

type MockNotionClient() =
    let post1 = 
       { id = "4d7ac503a7a64cc0ab757df70c7c7f7b"
         permalink = "test"
         title = "Test"
         summary = "This is a test"
         icon = ""
         iconAlt = ""
         cover = ""
         tags = [| "F#" |]
         createdAt = DateTimeOffset(DateTime(2022, 3, 11))
         updatedAt = DateTimeOffset(DateTime(2022, 3, 11)) }
    let post2 = 
       { id = "33f11309306447ce8a48e962f0e0d814"
         permalink = "another-test"
         title = "Another Test"
         summary = "This is another test"
         icon = ""
         iconAlt = ""
         cover = ""
         tags = [| "F#" |]
         createdAt = DateTimeOffset(DateTime(2022, 3, 11))
         updatedAt = DateTimeOffset(DateTime(2022, 3, 11)) }
    let lookupById = Map.ofList [
        post1.id, post1
        post2.id, post2
    ]
    let lookupByPermalink = Map.ofList [
        post1.permalink, post1
        post2.permalink, post2
    ]
    interface INotionClient with
        member _.List() = task {
            return [| post1; post2 |]
        }
        member _.GetById(pageId:string) = task {
            match lookupById |> Map.tryFind pageId with
            | Some properties ->
                return Some { properties = properties; content = [||] }
            | None ->
                return None
        }
        member _.GetByPermalink(permalink:string) = task {
            match lookupByPermalink |> Map.tryFind permalink with
            | Some properties ->
                return Some { properties = properties; content = [||] }
            | None ->
                return None
        }
        
