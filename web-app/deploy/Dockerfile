ARG RUNTIME_IMAGE

FROM node:12.16 as builder

WORKDIR /app

ARG REACT_APP_GRAPHQL_SCHEME=http
ARG REACT_APP_GRAPHQL_HOST=localhost
ARG REACT_APP_GRAPHQL_PORT=4000
ARG REACT_APP_SEGMENT_SOURCE=test

COPY package.json .
COPY package-lock.json .
RUN npm install

COPY tsconfig.json .
COPY src src
COPY public public
RUN npm run build

FROM ${RUNTIME_IMAGE} as runner

COPY --from=builder /app/build /var/www
COPY deploy/nginx.conf /etc/nginx/conf.d/default.conf
