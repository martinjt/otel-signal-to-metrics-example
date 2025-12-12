using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// Create ActivitySource for tracing
var activitySource = new ActivitySource("stm-test");

// Create Meter for metrics
var meter = new Meter("stm-test");

// Create metrics
var basketValueHistogram = meter.CreateHistogram<double>("basket.value", "USD", "The value of the shopping basket");
var basketCounter = meter.CreateCounter<long>("basket.count", "count", "The number of baskets processed");

// Configure OpenTelemetry
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("stm-test"))
    .AddSource(activitySource.Name)
    .AddOtlpExporter(options =>
    {
        options.BatchExportProcessorOptions.MaxExportBatchSize = 1;
    })
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("stm-test"))
    .AddMeter(meter.Name)
    .AddOtlpExporter((options, readerOptions) =>
    {
        readerOptions.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
    })
    .Build();

Console.WriteLine("Starting telemetry generation...");
Console.WriteLine("Sending telemetry to http://localhost:4317");
Console.WriteLine("Press Ctrl+C to stop.");
Console.WriteLine();

var random = new Random();

// Generate telemetry in a loop
for (var i = 0; i < 1000000000; i++)
{
    // Generate a random basket value between 10 and 500
    var basketValue = Math.Round(random.NextDouble() * 490 + 10, 2);

    // Create a span for the basket processing operation
    using (var activity = activitySource.StartActivity("ProcessBasket", ActivityKind.Internal))
    {
        // Add the basket value as an attribute to the span
        activity?.SetTag("basket.value", basketValue);
        activity?.SetTag("basket.currency", "USD");

        // Record the basket value metric
        basketValueHistogram.Record(basketValue);

        // Increment the basket counter
        basketCounter.Add(1);

        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Processed basket with value: ${basketValue:F2}");
    }

    // Wait for 1-3 seconds before generating the next basket
    await Task.Delay(random.Next(1, 3));
}

tracerProvider.Dispose();
meterProvider.Dispose();