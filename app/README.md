# Application
Client and server for [andrewmeier.dev](https://andrewmeier.dev) using
F#, Fable, Fable.Remoting, Feliz, Giraffe, and Airtable.

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
   Empty
   InstallClient
   PublishServer
   Restore
   StartClient
   StartServer
   TestUnits
```

Running the application
```
docker-compose up -d --build app
```
> Navigate to http://localhost:5000

## Resources
- [Fable.Remoting](https://zaid-ajaj.github.io/Fable.Remoting/)
- [Feliz](https://zaid-ajaj.github.io/Feliz/)
- [Giraffe](https://github.com/giraffe-fsharp/Giraffe)
- [Airtable](https://airtable.com/)
