# GraphQL API
[GraphQL](https://graphql.org/) API for [andrewmeier.dev](https://andrewmeier.dev).

## Setup
1. Install [.NET SDK](https://dotnet.microsoft.com/download)
2. Restore tools
    ```
    dotnet tool restore
    ```

## Development
Make changes then run the tests.
```
fake build -t Test
```

Once tests pass, run the server locally.
```
fake build -t Serve
```

Build the Docker image.
```
docker-compose build graphql-api
```

Run the Docker image.
```
docker-compose up -d graphql-api
```
> You can change the mounted secrets in the [docker-compose.yml](../docker-compose.yml) file.

## Resources
- [GraphQL](https://graphql.org/)
- [F# GraphQL](https://github.com/fsprojects/FSharp.Data.GraphQL)
- [Designing GraphQL Mutations](https://blog.apollographql.com/designing-graphql-mutations-e09de826ed97)
