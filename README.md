# Signal-to-Metrics Test Project

This project demonstrates converting OpenTelemetry spans to metrics using the signaltometrics connector in the OpenTelemetry Collector, specifically for HTTP semantic convention metrics.

***Note:** This generates much more granular histogram buckets that may cause a lot of expense in some platforms (each bucket is a time series), this is not the case for Honeycomb as it's a single time series regardless of the number of buckets.*

## Methodology

This is a simple HTTP API project that will generate some telemetry. Specifically, it will generate Metrics and Traces.

The main components are the Basket API, the Collector and the K6 load generator.

## Signal to metrics

The HTTP API generates it's own telemetry, however, on top of that, the collector will generate some of the important RED Metrics. These metrics are prefixed with `stm.`.

In addition, the collector also generates the `http.server.request.duration` metric as an exponential histogram for more granularity.

## Quick Start

## Environment Variables

Set in `.env` file:
```
HONEYCOMB_API_KEY=your_api_key_here
```

### Start the services

```bash
docker compose up -d --build
```

This starts:
- OpenTelemetry Collector on ports 4317 (gRPC) and 4318 (HTTP)
- Basket API on port 8080

### Run load test

```bash
K6_OTEL_GRPC_EXPORTER_INSECURE=true K6_OTEL_SERVICE_NAME=load-gen k6 run ./k6-loadtest.js -o opentelemetry
```

This generates telemetry for basket processing in a loop.

