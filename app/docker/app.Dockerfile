ARG RUNTIME_IMAGE_TAG=5.0
FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder

WORKDIR /app

# prevent sending metrics to microsoft
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# update packages
RUN apt update && apt upgrade -y

# install Node
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash - \
    && apt-get install -y nodejs

# restore tools
COPY .config .config
RUN dotnet tool restore

# install F# dependencies
COPY paket.dependencies paket.lock ./
RUN dotnet paket install
RUN dotnet paket restore

WORKDIR /app/src/Client

# install JavaScript dependencies
COPY src/Client/package.json src/Client/package-lock.json ./
RUN npm install

WORKDIR /app

# copy projects and build script
COPY src src
COPY fake.sh .
RUN chmod +x fake.sh

# set the runtime identifier
ARG RUNTIME_ID=linux-x64
ENV RUNTIME_ID ${RUNTIME_ID}

# run unit tests, build the client, and publish the server 
RUN ./fake.sh TestUnits
RUN ./fake.sh BuildClient
RUN ./fake.sh PublishServer

FROM mcr.microsoft.com/dotnet/runtime:${RUNTIME_IMAGE_TAG} as runner

WORKDIR /app

COPY --from=builder /app/src/Server/out .
COPY --from=builder /app/src/Client/out ./wwwroot

ENTRYPOINT ["dotnet", "Server.dll"]
