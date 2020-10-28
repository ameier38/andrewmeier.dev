# Web App
[Fable](https://fable.io/docs/) web application for Andrew's blog.

## Setup
Install dotnet tools.
```
dotnet tool install femto -g
dotnet tool install paket -g
dotnet tool install fake-cli -g
```

Install dependencies
```
fake build -t Install
```

## Development
Start the GraphQL API.
```
docker-compose up -d graphql-api
```

Run the development server.
```
fake build -t Serve
```

## Resources
- [Fable](https://fable.io/)
- [Feliz](https://zaid-ajaj.github.io/Feliz/)
- [Feliz.MaterialUI](https://github.com/cmeeren/Feliz.MaterialUI)
- [Feliz.Router](https://github.com/Zaid-Ajaj/Feliz.Router)
- [Prism](https://prismjs.com/extending.html#api)
