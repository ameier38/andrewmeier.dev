namespace Post

open System

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
    { Posts: PostDto list
      PageToken: string option }
