FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder

WORKDIR /app

# prevent sending metrics to microsoft
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# install tools
RUN dotnet tool install -g fake-cli
RUN dotnet tool install -g paket

# add tools to PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# install Node
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - \
    && apt-get install -y nodejs

# install dependencies
COPY package.json .
COPY package-lock.json .
COPY paket.dependencies .
COPY paket.lock .
COPY build.fsx .
RUN fake build -t Install

# build application
COPY src src
COPY dist dist
COPY .env .
COPY webpack.config.js .
RUN fake build -t Build

FROM caddy/caddy:alpine as runner

COPY deploy/Caddyfile /etc/caddy/Caddyfile

COPY --from=builder /app/dist /site
