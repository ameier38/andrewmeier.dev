FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as base

WORKDIR /app

# install Node
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - \
    && apt-get install -y nodejs

# install locales
RUN apt-get update && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y locales

RUN sed -i -e 's/# en_US.UTF-8 UTF-8/en_US.UTF-8 UTF-8/' /etc/locale.gen && \
    dpkg-reconfigure --frontend=noninteractive locales && \
    update-locale LANG=en_US.UTF-8

# set locales
ENV LANG en_US.UTF-8
ENV LANGUAGE en_US.UTF-8
ENV LC_ALL en_US.UTF-8

# prevent sending metrics to microsoft
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# install tools
RUN dotnet tool install -g fake-cli --version 5.20.4-alpha.1642 \
    && dotnet tool install -g paket --version 5.251.0 \
    && dotnet tool install -g femto

# add tools to PATH
ENV PATH="$PATH:/root/.dotnet/tools"

FROM base as builder

# install dependencies
COPY package.json .
COPY package-lock.json .
COPY paket.dependencies .
COPY paket.lock .
COPY build.fsx .
RUN fake build -t Install

# set build variables
ARG GRAPHQL_SCHEME=http
ARG GRAPHQL_HOST=localhost
ARG GRAPHQL_PORT=4000
ARG DISQUS_APP_SCHEME=http
ARG DISQUS_APP_HOST=localhost
ARG DISQUS_APP_PORT=8080
ARG DISQUS_SHORTNAME=andrewmeier-dev

# build application
COPY src src
COPY dist dist
COPY webpack.config.js .
RUN fake build -t Build

FROM nginx:1.17-alpine as runner

COPY --from=builder /app/dist /var/www
COPY nginx.conf /etc/nginx/conf.d/default.conf
