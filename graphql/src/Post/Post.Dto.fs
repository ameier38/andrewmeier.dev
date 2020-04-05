namespace Post

open System

type PostSummaryDto =
    { PostId: string
      Title: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

type PostDto =
    { PostId: string
      Title: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      Content: string }

type ListPostsInputDto =
    { PageSize: int option
      PageToken: string option }

type GetPostInputDto =
    { PostId: string }

type ListPostsResponseDto =
    { Posts: PostSummaryDto list
      PageToken: string option }
