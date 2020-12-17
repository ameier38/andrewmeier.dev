[<RequireQualifiedAccess>]
module Client.Page.Post

open Client
open Elmish
open Feliz
open Feliz.MaterialUI
open Shared.Api
open Shared.Domain

type Url =
    | EmptyUrl
    | PostUrl of permalink:string

type State =
    { CurrentUrl: Url
      Post: Deferred<Post> }

type Msg =
    | UrlChanged of Url
    | PostReceived of Post
    | ErrorReceived of exn

let getPost (permalink:string) =
    async {
        let req = { Permalink = permalink }
        let! res = Client.Api.postApi.getPost req
        return res.Post
    }

let init (url:Url): State * Cmd<Msg> =
    match url with
    | EmptyUrl ->
        let state =
            { CurrentUrl = url
              Post = HasNotStarted }
        state, Cmd.none
    | PostUrl permalink ->
        let state =
            { CurrentUrl = url
              Post = InProgress }
        state, Cmd.OfAsync.either getPost permalink PostReceived ErrorReceived

let update (msg:Msg) (state:State): State * Cmd<Msg> =
    match msg with
    | UrlChanged EmptyUrl ->
        state, Cmd.none
    | UrlChanged ((PostUrl newPermalink) as newUrl) ->
        match state.CurrentUrl with
        // don't refetch if the post is the same as current state
        | PostUrl prevPermalink when prevPermalink = newPermalink ->
            state, Cmd.none
        | EmptyUrl
        | PostUrl _ ->
            let state = { state with CurrentUrl = newUrl; Post = InProgress }
            let cmd = Cmd.OfAsync.either getPost newPermalink PostReceived ErrorReceived
            state, cmd
    | PostReceived post ->
        { state with Post = Resolved post }, Cmd.none
    | ErrorReceived err ->
        { state with Post = Error err.Message }, Cmd.none

let useStyles = Styles.makeStyles(fun styles theme ->
    {|
        card = styles.create [
            style.position.relative
        ]
        cardHeader = styles.create [
            style.position.absolute
            style.top 20
            style.left 20
            style.color color.white
            style.zIndex 1
        ]
        cardMedia = styles.create [
            style.height 250
            style.filter.brightness 50
            style.backgroundColor.darkGray
        ]
    |}
)

let renderDisqus (post:Post) =
    let config = Config.disqusConfig
    let hostPort =
        match config.AppPort with
        | "" | "80" -> config.AppHost
        | port -> sprintf "%s:%s" config.AppHost port
    let url = sprintf "%s://%s" config.AppScheme hostPort
    let postDisqusConfig: Disqus.DisqusConfig =
        { url = url
          identifier = post.Permalink
          title = post.Title }
    Mui.card [
        Mui.cardContent [
            Disqus.disqus [
                Disqus.Shortname config.Shortname
                Disqus.Config postDisqusConfig
            ]
        ]
    ]

type PostArgs =
    { post: Post }

let renderPost =
    React.functionComponent<PostArgs> (fun props ->
        let c = useStyles()
        let highlight (el:Browser.Types.Element) =
            match el with
            | null -> ()
            | el -> Prism.highlightAllUnder el
        let ref = React.useCallback(highlight)
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
                                            typography.children props.post.Title
                                        ]
                                        Mui.typography [
                                            typography.variant.subtitle1
                                            typography.children [
                                                Html.span "Created On: "
                                                Html.text (props.post.CreatedAt.ToString("yyyy-MM-dd"))
                                            ]
                                        ]
                                        Mui.typography [
                                            typography.variant.subtitle1
                                            typography.children [
                                                Html.span "Updated On: "
                                                Html.text (props.post.UpdatedAt.ToString("yyyy-MM-dd"))
                                            ]
                                        ]
                                    ]
                                ]
                                Mui.cardMedia [
                                    prop.className c.cardMedia
                                    cardMedia.image props.post.Cover
                                ]
                                Mui.cardContent [
                                    Mui.typography [
                                        typography.component' "div"
                                        prop.ref ref
                                        prop.className "post"
                                        prop.dangerouslySetInnerHTML (props.post.Content)
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
                        renderDisqus props.post
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

let renderError (msg:string) =
    Mui.dialog [
        Mui.dialogTitle "Error"
        Mui.dialogContent [
            Mui.dialogContentText msg
        ]
    ]

let render (state:State) (dispatch:Msg -> unit) =
    match state.Post with
    | HasNotStarted
    | InProgress ->
        renderSkeleton()
    | Resolved post ->
            renderPost { post = post }
    | Error err ->
        renderError err
