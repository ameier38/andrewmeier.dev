ARG RUNTIME_IMAGE_TAG=5.0
FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder

WORKDIR /app

# prevent sending metrics to microsoft
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# install Node
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - \
    && apt-get install -y nodejs

# install F# dependencies
COPY .config ./.config
COPY paket.dependencies .
COPY paket.lock .
RUN dotnet tool restore
RUN dotnet paket install
RUN dotnet paket restore

WORKDIR /app/client

# install JavaScript dependencies
COPY client/package.json .
COPY client/package-lock.json .
RUN npm install

WORKDIR /app

# copy projects 
COPY build ./build
COPY shared ./shared
COPY client ./client
COPY server ./server
COPY unit-tests ./unit-tests
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

COPY --from=builder /app/server/out .
COPY --from=builder /app/client/dist ./wwwroot

ENTRYPOINT ["dotnet", "Server.dll"]
