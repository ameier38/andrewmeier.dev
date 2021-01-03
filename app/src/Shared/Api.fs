module Shared.Api

open Domain

type ListPostsRequest =
    { PageSize: int option
      PageToken: string option }

type ListPostsResponse =
    { Posts: PostSummary list
      PageToken: string option }

type GetPostRequest = { Permalink: string }

type GetPostResponse = { Post: Post }

type IPostApi =
    { getPost: GetPostRequest -> Async<GetPostResponse>
      listPosts: ListPostsRequest -> Async<ListPostsResponse> }
