module Shared.Domain

open System

type PostSummary =
    { postId: string
      permalink: string
      title: string
      summary: string option
      updatedAt: DateTimeOffset }

type Post =
    { postId: string
      permalink: string
      title: string
      cover: string
      createdAt: DateTimeOffset
      updatedAt: DateTimeOffset
      content: string }
