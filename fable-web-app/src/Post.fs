[<RequireQualifiedAccess>]
module Blog.Post

open Elmish
open Feliz
open Feliz.MaterialUI
open Graphql

type Url =
    | EmptyUrl
    | PostUrl of permalink:string

type State =
    { CurrentUrl: Url
      Post: Deferred<Result<PostDto,string>> }

type Msg =
    | UrlChanged of Url
    | GetPost of AsyncOperation<string,Result<PostDto,string>>

let getPost (graphql:IGraphqlClient) (permalink:string): Cmd<Msg> =
    async {
        let! response = graphql.GetPost(permalink)
        return GetPost (Finished response)
    } |> Cmd.fromAsync

let init (url:Url): State * Cmd<Msg> =
    match url with
    | EmptyUrl ->
        let state =
            { CurrentUrl = url
              Post = NotStarted }
        state, Cmd.none
    | PostUrl permalink ->
        let state =
            { CurrentUrl = url
              Post = NotStarted }
        state, Cmd.ofMsg(GetPost (Started permalink))

let update (graphql:IGraphqlClient) (msg:Msg) (state:State): State * Cmd<Msg> =
    match msg with
    | UrlChanged EmptyUrl ->
        state, Cmd.none
    | UrlChanged (PostUrl newPermalink) ->
        match state.CurrentUrl with
        // don't refetch if the post is the same as current state
        | PostUrl prevPermalink when prevPermalink = newPermalink ->
            state, Cmd.none
        | EmptyUrl
        | PostUrl _ ->
            state, Cmd.ofMsg(GetPost (Started newPermalink))
    | GetPost (Started postId) ->
        { state with Post = InProgress }, getPost graphql postId
    | GetPost (Finished response) ->
        { state with Post = Resolved response }, Cmd.none

let useStyles = Styles.makeStyles(fun styles theme ->
    {|
        card = styles.create [
            style.position.relative
        ]
        cardHeader = styles.create [
            style.position.absolute
            style.top 20
            style.left 20
            style.color theme.palette.primary.contrastText
            style.zIndex 1
        ]
        cardMedia = styles.create [
            style.height 250
            style.filter.brightness 50
            style.backgroundColor.darkGray
        ]
    |}
)

let renderDisqus (post:PostDto) =
    let shortname = Env.getEnv "FABLE_APP_DISQUS_SHORTNAME"
    let scheme = Env.getEnv "FABLE_APP_SCHEME"
    let host = Env.getEnv "FABLE_APP_HOST"
    let port = Env.getEnv "FABLE_APP_PORT"
    let url =
        match port with
        | "" | "80" -> sprintf "%s://%s" scheme host
        | port -> sprintf "%s://%s:%s" scheme host port
    let config: Disqus.DisqusConfig =
        { url = url
          identifier = post.permalink
          title = post.title }
    Mui.card [
        Mui.cardContent [
            Disqus.disqus [
                Disqus.Shortname shortname
                Disqus.Config config
            ]
        ]
    ]

let renderPost =
    React.functionComponent<PostDto> (fun props ->
        let c = useStyles()
        let highlight (el:Browser.Types.Element) =
            match el with
            | null -> ()
            | el -> Prism.highlightAllUnder el
        let ref = React.useCallback(highlight, [| props.content |> box |])
        Mui.grid [
            grid.container true
            grid.spacing._2
            grid.children [
                Mui.grid [
                    grid.item true
                    grid.xs._12
                    grid.children [
                        Mui.card [
                            prop.className c.card
                            card.children [
                                Html.div [
                                    prop.className c.cardHeader
                                    prop.children [
                                        Mui.typography [
                                            typography.variant.h4
                                            typography.children props.title
                                        ]
                                        Mui.typography [
                                            typography.variant.subtitle1
                                            typography.children [
                                                Html.span "Created On: "
                                                Html.text (props.createdAt.ToString("yyyy-MM-dd"))
                                            ]
                                        ]
                                        Mui.typography [
                                            typography.variant.subtitle1
                                            typography.children [
                                                Html.span "Updated On: "
                                                Html.text (props.updatedAt.ToString("yyyy-MM-dd"))
                                            ]
                                        ]
                                    ]
                                ]
                                Mui.cardMedia [
                                    prop.className c.cardMedia
                                    cardMedia.image props.cover
                                ]
                                Mui.cardContent [
                                    Mui.typography [
                                        prop.ref ref
                                        prop.className "post"
                                        prop.dangerouslySetInnerHTML (props.content)
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                Mui.grid [
                    grid.item true
                    grid.xs._12
                    grid.children [
                        renderDisqus props
                    ]
                ]
            ]
        ]
    )

let renderSkeleton () =
    Mui.card [
        card.children [
            Mui.skeleton [
                prop.height 200
                skeleton.variant.rect
                skeleton.animation.wave
            ]
            Html.div [
                prop.style [
                    style.padding 30
                ]
                prop.children [
                    yield Mui.skeleton [
                        skeleton.component' "h2"
                        skeleton.animation.wave
                        skeleton.width 200
                    ]
                    for _ in 1..10 do
                        yield Mui.skeleton [
                            skeleton.component' "p"
                            skeleton.animation.wave
                        ]
                ]
            ]
        ]
    ]

let render (state:State) (dispatch:Msg -> unit) =
    match state.Post with
    | NotStarted
    | InProgress ->
        renderSkeleton()
    | Resolved response ->
        match response with
        | Ok post ->
            renderPost post
        | Error error ->
            Error.renderError error
