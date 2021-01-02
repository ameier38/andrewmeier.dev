module Client.Config

let disqusConfig =
    {| AppScheme = Env.getEnv "DISQUS_APP_SCHEME" "http"
       AppHost = Env.getEnv "DISQUS_APP_HOST" "localhost"
       AppPort = Env.getEnv "DISQUS_APP_PORT" "8080"
       Shortname = Env.getEnv "DISQUS_SHORTNAME" "andrewmeier-dev" |}
