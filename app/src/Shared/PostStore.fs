module Shared.PostStore

open Domain

type ListPostsRequest = { pageSize: int option; pageToken: string option }

type ListPostsResponse = { posts: PostSummary list; pageToken: string option }

type GetPostRequest = { permalink: string }

type GetPostResponse = { post: Post }

type IPostStore =
    { getPost: GetPostRequest -> Async<GetPostResponse>
      listPosts: ListPostsRequest -> Async<ListPostsResponse> }
