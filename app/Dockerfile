ARG RUNTIME_IMAGE_TAG=6.0-bullseye-slim
FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim as builder

WORKDIR /app

# prevent sending metrics to microsoft
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# update packages
RUN apt-get update && apt-get upgrade -y

# install tailwindcss CLI
RUN curl -sLO https://github.com/tailwindlabs/tailwindcss/releases/download/v3.3.1/tailwindcss-linux-x64 \
    && chmod +x tailwindcss-linux-x64 \
    && mv tailwindcss-linux-x64 /usr/local/bin/tailwindcss

# restore tools
COPY .config .config
RUN dotnet tool restore

# install F# dependencies
COPY paket.dependencies paket.lock ./
RUN dotnet paket install && dotnet paket restore

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
