using System.Net;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Mimo.Challenge.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Enables sending traces, metrics and logs to OpenTelemetry endpoint.
    /// Configured via environment variables https://opentelemetry.io/docs/languages/sdk-configuration/general/
    /// Necessary variables: OTEL_EXPORTER_OTLP_ENDPOINT, OTEL_SERVICE_NAME
    /// </summary>
    public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder builder)
    {
        // Setup logging to be exported via OpenTelemetry
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var otelBuilder = builder.Services.AddOpenTelemetry();

        // Add Metrics for ASP.NET Core and our custom metrics and export via OTLP
        otelBuilder.WithMetrics(metrics =>
        {
            // Metrics provider from OpenTelemetry
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddRuntimeInstrumentation();

            // Metrics provided by ASP.NET Core in .NET 8
            metrics
                .AddMeter("Microsoft.AspNetCore.Http")
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddMeter("Microsoft.AspNetCore.Http.Connections")
                .AddMeter("Microsoft.AspNetCore.Routing")
                .AddMeter("Microsoft.AspNetCore.Diagnostics")
                .AddMeter("Microsoft.AspNetCore.RateLimiting");
        });

        // Add Tracing for ASP.NET Core and our custom ActivitySource and export via OTLP
        otelBuilder.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
        });

        // Export OpenTelemetry data via OTLP, using env vars for the configuration
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (otlpEndpoint != null)
        {
            otelBuilder.UseOtlpExporter();
        }

        return builder;
    }
}
