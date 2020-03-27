# Andrew Meier's blog
Personal blog of Andrew C. Meier. Built using 
[Fable](), [Elmish](), [Feliz](). Hosted by
a bunch of Raspberry Pi's in my apartment.

## Setup
Install [.NET Core SDK](https://dotnet.microsoft.com/download).
```
sudo choco install dotnetcore-sdk -y
```

Install paket, fake, and femto global tools.
```
dotnet tool install fake-cli -g
dotnet tool install paket -g
```

Install the .NET dependencies.
```
fake build -t Restore
```

Install the JavaScript dependencies.
```
fake build -t Install
```

## Development
Start the development server.
```
fake build -t Start
```