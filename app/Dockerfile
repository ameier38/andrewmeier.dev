ARG RUNTIME_IMAGE_TAG=6.0
FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder

WORKDIR /app

# prevent sending metrics to microsoft
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# update packages
RUN apt-get update && apt-get upgrade -y

# install Node (needed to build Tailwind CSS)
RUN curl -sL https://deb.nodesource.com/setup_16.x | bash - \
    && apt-get install -y nodejs

# restore tools
COPY .config .config
RUN dotnet tool restore

# install F# dependencies
COPY paket.dependencies paket.lock ./
RUN dotnet paket install
RUN dotnet paket restore

WORKDIR /app/src/Server

# install JavaScript dependencies
COPY src/Server/package.json src/Server/package-lock.json ./
RUN npm install

WORKDIR /app

# copy projects and build script
COPY src src
COPY fake.sh .

# set the runtime identifier
ARG RUNTIME_ID=linux-x64
ENV RUNTIME_ID ${RUNTIME_ID}

# publish the server 
RUN ./fake.sh Publish

FROM mcr.microsoft.com/dotnet/aspnet:${RUNTIME_IMAGE_TAG} as runner

WORKDIR /app

# copy compiled code from build image
COPY --from=builder /app/src/Server/out .

ENTRYPOINT ["dotnet", "Server.dll"]
