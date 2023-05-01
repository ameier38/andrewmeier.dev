# Application
Server for [andrewmeier.dev](https://andrewmeier.dev).

## Setup
1. Install [.NET SDK](https://dotnet.microsoft.com/download).
2. Install [TailwindCSS CLI](https://github.com/tailwindlabs/tailwindcss/releases/tag/v3.2.4).
3. Install development certificates.
   ```shell
   dotnet dev-certs https --trust
   ```
4. Restore tools
    ```shell
    dotnet tool restore
    ```
5. Install packages.
   ```shell
   dotnet paket install
   dotnet paket restore
   ```

## Usage
List build targets.
```shell
.\fake.cmd
```
```
The following targets are available:
   BuildTailwind
   Publish
   Test
   Watch
```

Watch the server for local development.
```shell
./fake.cmd Watch
```
> Navigate to http://localhost:5000.

Run the application locally.
```shell
docker compose up -d --build app
```
> Navigate to http://localhost:5000
