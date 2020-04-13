ARG RUNTIME_IMAGE

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

# set build variables
ARG FABLE_APP_SCHEME=http
ARG FABLE_APP_HOST=localhost
ARG FABLE_APP_PORT=8080
ARG FABLE_APP_GRAPHQL_SCHEME=http
ARG FABLE_APP_GRAPHQL_HOST=localhost
ARG FABLE_APP_GRAPHQL_PORT=4000
ARG FABLE_APP_DISQUS_SHORTNAME=andrewmeier-dev

# build application
COPY src src
COPY dist dist
COPY webpack.config.js .
RUN fake build -t Build

FROM ${RUNTIME_IMAGE} as runner

COPY --from=builder /app/dist /var/www
COPY deploy/nginx.conf /etc/nginx/conf.d/default.conf
