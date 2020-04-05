FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as base

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
RUN dotnet tool install -g fake-cli
RUN dotnet tool install -g paket

# add tools to PATH
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /app

# copy paket dependencies
COPY paket.dependencies .
COPY paket.lock .

# copy build script
COPY build.fsx .

# install dependencies
RUN fake build

FROM base as builder

COPY src ./src
RUN fake build -t Publish -e runtime=linux-musl-x64

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine as runner

COPY --from=builder /app/src/Server/out/graphql /usr/local/bin/graphql
RUN chmod +x /usr/local/bin/graphql

ENTRYPOINT ["graphql"]
