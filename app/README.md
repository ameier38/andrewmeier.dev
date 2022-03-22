# Application
Server for [andrewmeier.dev](https://andrewmeier.dev).

## Setup
1. Install [.NET SDK](https://dotnet.microsoft.com/download)
2. Restore tools
    ```
    dotnet tool restore
    ```
3. Install packages.
   ```
   dotnet paket install
   dotnet paket restore
   ```

## Usage
List build targets.
```powershell
.\fake.cmd
```
```
The following targets are available:
   BuildTailwind
   Clean
   Publish
   Restore
   Test
   Watch
   WatchServer
   WatchTailwind
```

Watch the server for local development.
```
./fake.cmd Watch
```
> Navigate to http://localhost:5000.

Run the application locally.
```
docker compose up -d --build app
```
> Navigate to http://localhost:5000
