module Shared.Domain

open System

type PostSummary =
    { PostId: string
      Permalink: string
      Title: string
      CreatedAt: DateTimeOffset }

type Post =
    { PostId: string
      Permalink: string
      Title: string
      Cover: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      Content: string }
