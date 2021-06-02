module Client.Config

let envPrefix = "SNOWPACK_PUBLIC"

let disqusConfig =
    {| AppScheme = Env.getEnv $"{envPrefix}_DISQUS_APP_SCHEME" "http"
       AppHost = Env.getEnv $"{envPrefix}_DISQUS_APP_HOST" "localhost"
       AppPort = Env.getEnv $"{envPrefix}_DISQUS_APP_PORT" "8080"
       Shortname = Env.getEnv $"{envPrefix}_DISQUS_SHORTNAME" "andrewmeier-dev" |}
