# Web App
[Fable](https://fable.io/docs/) web application for Andrew's blog.

## From scratch
1. Add the dotnet templates.
    ```
    dotnet new -i Fable.Template
    dotnet new -i fake-template
    ```
2. Install dotnet tools.
    ```
    dotnet tool install femto -g
    dotnet tool install paket -g
    dotnet tool install fake-cli -g
    ```
3. Create a Fable project.
    ```
    dotnet new fable -n App -o .
    ```
    > Remove the package references from the project file as we will use Paket.
4. Add [FAKE](https://fake.build/) script.
    ```
    dotnet new fake -b none -d file -ds buildtask
    ```
5. Install dependencies.
    ```
    cd src
    femto install Fable.Elmish
    femto install Fable.Elmish.React
    femto install Feliz
    femto install Feliz.MaterialUI
    femto install Feliz.Router
    ```
    > `femto` installs the required .NET and Node packages.
6. Add [graphql-hooks](https://github.com/nearform/graphql-hooks).
    ```
    npm install graphql-hooks
    ```

## Resources
- [Fable](https://fable.io/)
- [Feliz](https://zaid-ajaj.github.io/Feliz/)
- [Feliz.MaterialUI](https://github.com/cmeeren/Feliz.MaterialUI)
- [Feliz.Router](https://github.com/Zaid-Ajaj/Feliz.Router)
- [Prism](https://prismjs.com/extending.html#api)
