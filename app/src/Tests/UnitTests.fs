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
        let req:ListPostsRequest = { pageSize = Some 10; pageToken = None }
        // WHEN we send the request
        let! res = mockPostStore.listPosts(req)
        // THEN we should receive posts
        let expectedPermalinks = ["win-dev"; "about"]
        let actualPermalinks = res.posts |> List.map (fun p -> p.permalink)
        Expect.sequenceEqual actualPermalinks expectedPermalinks "permalinks should match"
    }
    
let tests =
    testList "Tests" [
        testPostStore
    ]

let run () =
    runTests defaultConfig tests
