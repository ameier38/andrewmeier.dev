open Expecto
open Markdig
open Shared.Api
open Server.Api
open Server.PostClient

let markdownPipeline = MarkdownPipelineBuilder().Build() 

let postClient = MockPostClient()

let mockApi = postApi postClient

[<Tests>]
let testPostApi =
    testAsync "PostApi" {
        // GIVEN a list posts request
        let req:ListPostsRequest = { PageSize = Some 10; PageToken = None }
        // WHEN we send the request
        let! res = mockApi.listPosts(req)
        // THEN we should receive posts
        let expectedPermalinks = ["about"; "win-dev"]
        let actualPermalinks = res.Posts |> List.map (fun p -> p.Permalink)
        Expect.sequenceEqual actualPermalinks expectedPermalinks "permalinks should match"
    }

[<EntryPoint>]
let main argv = runTestsInAssembly defaultConfig argv