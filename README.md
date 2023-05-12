[![Merge](https://github.com/ameier38/andrewmeier.dev/actions/workflows/merge.yml/badge.svg)](https://github.com/ameier38/andrewmeier.dev/actions/workflows/merge.yml)

# andrewmeier.dev
Repo for [Andrew's blog](https://andrewmeier.dev).

Consists of a single server built using F#. Server generates plain HTML so there
is no need for a front-end framework like React which greatly simplifies things.

Built using:
- [F#](https://fsharp.org/), [Giraffe](https://github.com/giraffe-fsharp/Giraffe): Web server that generates HTML using F#. 
- [Notion .NET](https://github.com/notion-dotnet/notion-sdk-net): Blog posts are written using Notion and then fetched by the server using the Notion API.
- [htmx](https://htmx.org/): Client framework which allows you to make requests directly from HTML instead of JavaScript.
- [Tailwind CSS](https://tailwindcss.com/): CSS styles.
- [Pulumi](https://www.pulumi.com/): Deployment. Currently running on a Raspberry Pi in my apartment.
