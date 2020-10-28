module Server.Post.Dto

open System

type PostSummaryDto =
    { PostId: string
      Permalink: string
      Title: string
      CreatedAt: DateTimeOffset }

type PostDto =
    { PostId: string
      Permalink: string
      Title: string
      Cover: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      Content: string }

type ListPostsInputDto =
    { PageSize: int option
      PageToken: string option }

type GetPostInputDto =
    { Permalink: string }

type ListPostsResponseDto =
    { Posts: PostSummaryDto list
      PageToken: string option }
