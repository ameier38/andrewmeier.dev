# andrewmeier.dev
Repo for [Andrew's blog](https://andrewmeier.dev).

Consists of a single server built (almost) entirely using F#. Server generates plain html so there
is no need for a front-end framework like React which greatly simplifies things.

Built using:
- [F#](https://fsharp.org/), [ASP.NET](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-5.0), and [Feliz.ViewEngine](https://github.com/dbrattli/Feliz.ViewEngine): Web server that generates HTML using F#. 
- [Notion .NET](https://github.com/notion-dotnet/notion-sdk-net): Blog posts are written using Notion and then fetched by the server using the Notion API.
- [htmx](https://htmx.org/): Client framework which allows you to make requests directly from HTML instead of JavaScript.
- [Tailwind CSS](https://tailwindcss.com/): CSS styles.
- [Pulumi](https://www.pulumi.com/): Deployment. Currently running on a Raspberry Pi in my apartment.
