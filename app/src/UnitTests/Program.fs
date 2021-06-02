open Expecto
open Markdig
open Shared.PostStore
open Server.PostStore
open Server.PostClient

let markdownPipeline = MarkdownPipelineBuilder().Build() 

let postClient = MockPostClient()

let mockPostStore = postStore postClient

[<Tests>]
let testPostStore =
    testAsync "PostStore" {
        // GIVEN a list posts request
        let req:ListPostsRequest = { PageSize = Some 10; PageToken = None }
        // WHEN we send the request
        let! res = mockPostStore.listPosts(req)
        // THEN we should receive posts
        let expectedPermalinks = ["win-dev"; "about"]
        let actualPermalinks = res.Posts |> List.map (fun p -> p.Permalink)
        Expect.sequenceEqual actualPermalinks expectedPermalinks "permalinks should match"
    }

[<EntryPoint>]
let main argv = runTestsInAssembly defaultConfig argv