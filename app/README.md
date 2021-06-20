# Application
Client and server for [andrewmeier.dev](https://andrewmeier.dev) using
F#, ASP.NET, Fable, Fable.Remoting, Feliz, Tailwind CSS, Snowpack, and Airtable.

## Setup
1. Install [.NET SDK](https://dotnet.microsoft.com/download)
2. Restore tools
    ```
    dotnet tool restore
    ```

## Usage
List build targets.
```powershell
.\fake.cmd
```
```
The following targets are available:
   BuildClient
   Clean
   CleanClient
   InstallClient
   PublishTests
   PublishServer
   Restore
   TestIntegrations
   TestIntegrationsHeadless
   TestUnits
   Watch
   WatchClient
   WatchServer
```

Watch the server and client for local development.
```
./fake.cmd Watch
```
> Navigate to http://localhost:3000.
Server code changes will automatically rebuild the server.
Client code changes will automatically hot reload

Run the application locally.
```
docker-compose up -d --build app
```
> Navigate to http://localhost:5000

## Resources
- [Fable.Remoting](https://zaid-ajaj.github.io/Fable.Remoting/)
- [Feliz](https://zaid-ajaj.github.io/Feliz/)
- [Snowpack](https://www.snowpack.dev/)
- [Tailwind CSS](https://tailwindcss.com/)
- [Airtable](https://airtable.com/)
