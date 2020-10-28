ARG RUNTIME_IMAGE

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder

ARG PAKET_VERSION=5.250.0
ARG FAKE_VERSION=5.20.3

WORKDIR /app

# prevent sending metrics to microsoft
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# install tools
RUN dotnet tool install -g fake-cli --version ${FAKE_VERSION}
RUN dotnet tool install -g paket --version ${PAKET_VERSION}

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
ARG APP_SCHEME=http
ARG APP_HOST=localhost
ARG APP_PORT=8080
ARG GRAPHQL_SCHEME=http
ARG GRAPHQL_HOST=localhost
ARG GRAPHQL_PORT=4000
ARG DISQUS_SHORTNAME=andrewmeier-dev

# build application
COPY src src
COPY dist dist
COPY webpack.config.js .
RUN fake build -t Build

FROM ${RUNTIME_IMAGE} as runner

COPY --from=builder /app/dist /var/www
COPY nginx.conf /etc/nginx/conf.d/default.conf
