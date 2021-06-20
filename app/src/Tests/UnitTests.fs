module Tests.UnitTests

open Expecto
open Shared.PostStore
open Server.PostStore
open Server.PostClient

let postClient = MockPostClient()

let mockPostStore = postStore postClient

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
    
let tests =
    testList "Tests" [
        testPostStore
    ]

let run () =
    runTests defaultConfig tests
