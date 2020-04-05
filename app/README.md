# Andrew's Blog Application
Web application for Andrew's blog. Built using [React](https://create-react-app.dev/),
[TypeScript](https://www.typescriptlang.org/), and [Material-UI](https://material-ui.com/).

## Setup
Install nvm (Node version manager).
```
scoop install nvm
```

Install Node.
```
nvm install 12.16.1
nvm use 12.16.1
```
> You can use `nvm list available` to see available versions.

Install dependencies.
```
npm install
```

## Run locally
Start the development server.
```
npm start
```

## Development
If you update the GraphQL API then you should regenerate the types.
```
npm run generate
```
