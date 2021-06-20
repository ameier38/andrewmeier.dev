ARG RUNTIME_IMAGE_TAG=5.0
FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder

WORKDIR /app

# prevent sending metrics to microsoft
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# update packages
RUN apt update && apt upgrade -y

# restore tools
COPY .config .config
RUN dotnet tool restore

# install F# dependencies
COPY paket.dependencies paket.lock ./
RUN dotnet paket install
RUN dotnet paket restore

# copy projects and build script
COPY src src
COPY fake.sh .
RUN chmod +x fake.sh

# set the runtime identifier
ARG RUNTIME_ID=linux-x64
ENV RUNTIME_ID ${RUNTIME_ID}

# publish the test project
RUN ./fake.sh PublishTests

FROM mcr.microsoft.com/dotnet/runtime:${RUNTIME_IMAGE_TAG} as runner

# install packages needed for chrome driver
RUN apt-get update \
    && apt-get -y install --no-install-recommends \
        libgdiplus \
    # clean up
    && apt-get autoremove -y \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=builder /app/src/Tests/out .

ENTRYPOINT ["dotnet", "Tests.dll", "integration", "--remote"]
