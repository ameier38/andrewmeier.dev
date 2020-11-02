module Blog.Config

let disqusConfig =
    {| AppScheme = Env.getEnv "DISQUS_APP_SCHEME" "http"
       AppHost = Env.getEnv "DISQUS_APP_HOST" "localhost"
       AppPort = Env.getEnv "DISQUS_APP_PORT" "8080"
       Shortname = Env.getEnv "DISQUS_SHORTNAME" "andrewmeier-dev" |}

let graphqlConfig =
    {| Scheme = Env.getEnv "GRAPHQL_SCHEME" "http"
       Host = Env.getEnv "GRAPHQL_HOST" "localhost"
       Port = Env.getEnv "GRAPHQL_PORT" "4000" |}
