FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder

WORKDIR /app

# install dotnet tools
RUN dotnet tool install -g fake-cli
RUN dotnet tool install -g paket
ENV PATH="$PATH:/root/.dotnet/tools"

# install Node
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash
RUN apt-get install -y nodejs

COPY package.json package.json
COPY package-lock.json package-lock.json
COPY paket.dependencies paket.dependencies
COPY paket.lock packet.lock
COPY build.fsx build.fsx

RUN fake build -t Install

COPY src src
COPY dist dist
COPY webpack.config.js webpack.config.js

RUN fake build -t Build

FROM arm32v7/nginx as runner

COPY --from=builder /app/dist /usr/share/nginx/html
COPY deploy/nginx.conf /etc/nginx/conf.d/nginx.conf
