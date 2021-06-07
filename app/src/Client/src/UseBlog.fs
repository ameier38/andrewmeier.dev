module Client.UseBlog

open Elmish
open Fable.Remoting.Client
open Feliz
open Feliz.UseElmish
open Shared.Domain
open Shared.PostStore

let routeBuilder (typeName:string) (methodName:string) = $"/api/%s{typeName}/%s{methodName}"

let postApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.buildProxy<IPostStore>
    
let getPost (permalink:string) =
    async {
        let req = { Permalink = permalink }
        let! res = postApi.getPost req
        return res.Post
    }
    
let listPosts () =
    let rec recurse (posts:PostSummary list) (pageToken:string option) =
        async {
            let req = { PageSize = Some 10; PageToken = pageToken }
            let! res = postApi.listPosts req
            let posts = res.Posts @ posts
            match res.PageToken with
            | None -> return posts
            | Some _ -> return! recurse posts res.PageToken
        }
    recurse [] None

type State =
    { SelectedPost: Deferred<Post>
      Posts: Deferred<PostSummary list> }

type Msg =
    | LoadPost of permalink:string
    | LoadPostCompleted of Post
    | LoadPostFailed of exn
    | LoadPosts
    | LoadPostsCompleted of PostSummary list
    | LoadPostsFailed of exn
    
let init () =
    { SelectedPost = HasNotStarted
      Posts = HasNotStarted },
    Cmd.none
    
let update (msg:Msg) (state:State) =
    printfn $"msg: {msg}"
    match msg with
    | LoadPost permalink ->
        match state.SelectedPost with
        | Resolved { Permalink = currentPermalink } when permalink = currentPermalink ->
            state,
            Cmd.none
        | _ ->
            { state with SelectedPost = InProgress },
            Cmd.OfAsync.either getPost permalink LoadPostCompleted LoadPostFailed
    | LoadPostCompleted post ->
        { state with SelectedPost = Resolved post },
        Cmd.none
    | LoadPostFailed exn ->
        { state with SelectedPost = Error exn.Message },
        Cmd.none
    | LoadPosts ->
        { state with Posts = InProgress },
        Cmd.OfAsync.either listPosts () LoadPostsCompleted LoadPostsFailed
    | LoadPostsCompleted posts ->
        { state with Posts = Resolved posts },
        Cmd.none
    | LoadPostsFailed exn ->
        { state with Posts = Error exn.Message },
        Cmd.none
        
type BlogProviderValue =
    { State: State
      loadPost: string -> unit }
        
module BlogProvider =
    let blogContext = React.createContext<BlogProviderValue>("BlogContext")
    
    [<ReactComponent>]
    let BlogProvider (children:seq<ReactElement>) =
        let state, dispatch = React.useElmish(init, update)
        let init () = dispatch LoadPosts
        React.useEffectOnce(init)
        let blogProviderValue =
            { State = state
              loadPost = fun permalink -> dispatch (LoadPost permalink) }
        React.contextProvider(blogContext, blogProviderValue, children)
        
type Blog =
    static member inline provider (children:seq<ReactElement>) =
        BlogProvider.BlogProvider(children)
        
module React =
    let useBlog() = React.useContext(BlogProvider.blogContext)
