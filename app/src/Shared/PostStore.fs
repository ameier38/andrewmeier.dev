module Shared.PostStore

open Domain

type ListPostsRequest =
    { PageSize: int option
      PageToken: string option }

type ListPostsResponse =
    { Posts: PostSummary list
      PageToken: string option }

type GetPostRequest = { Permalink: string }

type GetPostResponse = { Post: Post }

type IPostStore =
    { getPost: GetPostRequest -> Async<GetPostResponse>
      listPosts: ListPostsRequest -> Async<ListPostsResponse> }
