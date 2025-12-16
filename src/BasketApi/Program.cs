using System.Diagnostics;
using BasketApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("basket-api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddOtlpExporter()
    )
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter((options, reader) =>
        {
            reader.PeriodicExportingMetricReaderOptions = new()
            {
                ExportIntervalMilliseconds = 60000,
                
            };
            reader.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
        })
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add middleware to track request/response body sizes
app.UseHttpBodySizeTracking();

// Create ActivitySource for custom spans
var activitySource = new ActivitySource("basket-api");

// Random number generator for basket values and errors
var random = new Random();

app.MapGet("/basket", async () =>
{
    // Generate a random basket value between 10 and 500
    var basketValue = Math.Round(random.NextDouble() * 490 + 10, 2);
    var itemCount = random.Next(1, 20);

    // Add the basket value as an attribute to the span
    Activity.Current?.SetTag("basket.value", basketValue);
    Activity.Current?.SetTag("basket.items.count", itemCount);
    Activity.Current?.SetTag("basket.currency", "USD");

    // Randomly throw an error (10% chance)
    if (random.Next(100) < 10)
    {
        Activity.Current?.SetTag("error.type", "BasketProcessingException");
        Activity.Current?.SetStatus(ActivityStatusCode.Error, "Failed to process basket");
        Activity.Current?.AddException(new Exception("Random basket processing failure"));

        return Results.Problem(
            title: "Basket Processing Failed",
            detail: "An unexpected error occurred while processing the basket",
            statusCode: 500
        );
    }

    // random delay
    await Task.Delay(random.Next(1, 3000));
    

    return Results.Ok(new
    {
        BasketId = Guid.NewGuid(),
        Value = basketValue,
        Currency = "USD",
        ItemCount = itemCount,
        Timestamp = DateTime.UtcNow
    });
})
.WithName("GetBasket");

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.Run();
