# Base Service Helm Chart
Helm chart to use for applications.

## gRPC
Applications that use gRPC must expose port `50051`
and implement the [gRPC Health Probe](https://github.com/grpc-ecosystem/grpc-health-probe).

## HTTP
Applications that use HTTP must expose port `8080`.
