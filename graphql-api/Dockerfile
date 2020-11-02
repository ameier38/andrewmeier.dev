FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as base

WORKDIR /app

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
    && dotnet tool install -g paket --version 5.251.0

# add tools to PATH
ENV PATH="$PATH:/root/.dotnet/tools"

FROM base as builder

# install dependencies
COPY paket.dependencies .
COPY paket.lock .
RUN paket install

# copy build script
COPY build.fsx .

COPY src ./src
RUN fake build -t Publish

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 as runner

WORKDIR /app

COPY --from=builder /app/src/Server/out .

ENTRYPOINT ["dotnet", "Server.dll"]
