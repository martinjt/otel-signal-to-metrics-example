# Signal To Metrics Test

This repo shows how you can use the Signal to Metrics connector in the collector to replicate generating metrics directly in your application.

***Note:** This generates much more granular histogram buckets that may cause a lot of expense in some platforms (each bucket is a time series), this is not the case for Honeycomb as it's a single time series regardless of the number of buckets.*

## Running

Add a `.env` with your HONEYCOMB_API_KEY.

```bash
docker-compose up -d
```

```bash
dotnet run --project src/SignalToMetricsTest
```

Ctrl-C to stop.
