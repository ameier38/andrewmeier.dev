module Post.Fields

open FSharp.Data.GraphQL.Types

let postIdInputField =
    Define.Input(
        name = "postId",
        typedef = ID,
        description = "Unique identifier of the post")

let pageSizeInputField =
    Define.Input(
        name = "pageSize",
        typedef = Nullable Int,
        description = "Number of posts to return")

let pageTokenField =
    Define.Input(
        name = "pageToken",
        typedef = Nullable ID,
        description = "Token for next page of posts")

let ListPostsInputObject =
    Define.InputObject<ListPostsInputDto>(
        name = "ListPostsInput",
        description = "ListPosts arguments",
        fields = [ pageSizeInputField; pageTokenField ])

let GetPostInputObject =
    Define.InputObject<GetPostInputDto>(
        name = "GetPostInput",
        description = "GetPost arguments",
        fields = [ postIdInputField ])

let PostSummaryType =
    Define.Object<PostSummaryDto>(
        name = "PostSummary",
        description = "Post summary",
        fields = [
            Define.AutoField("postId", ID)
            Define.AutoField("title", String)
            Define.Field("createdAt", Date, fun _ p -> p.CreatedAt.UtcDateTime)
        ]
    )

let PostType =
    Define.Object<PostDto>(
        name = "Post",
        description = "Post",
        fields = [
            Define.AutoField("postId", ID)
            Define.AutoField("title", String)
            Define.AutoField("cover", String)
            Define.Field("createdAt", Date, fun _ p -> p.CreatedAt.UtcDateTime)
            Define.Field("updatedAt", Date, fun _ p -> p.UpdatedAt.UtcDateTime)
            Define.AutoField("content", String)
        ])

let ListPostsResponseType =
    Define.Object<ListPostsResponseDto>(
        name = "ListPostsResponse",
        description = "List posts response",
        fields = [
            Define.Field("posts", ListOf PostSummaryType, fun _ res -> res.Posts)
            Define.AutoField("pageToken", String)
        ])

let listPostsField
    (postClient:IPostClient) =
    Define.Field(
        name = "listPosts",
        typedef = ListPostsResponseType,
        description = "List posts",
        args = [Define.Input("input", ListPostsInputObject)],
        resolve = (fun ctx _ ->
            let listPostsInput = ctx.Arg<ListPostsInputDto>("input")
            let pageSize = listPostsInput.PageSize |> Option.defaultValue 10
            postClient.ListPosts(pageSize, listPostsInput.PageToken)))

let getPostField
    (postClient:IPostClient) =
    Define.Field(
        name = "getPost",
        typedef = PostType,
        description = "Get post",
        args = [Define.Input("input", GetPostInputObject)],
        resolve = (fun ctx _ ->
            let getPostInput = ctx.Arg<GetPostInputDto>("input")
            postClient.GetPost(getPostInput.PostId)))
