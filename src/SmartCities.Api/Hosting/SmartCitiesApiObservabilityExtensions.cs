using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Registers and enables vendor-neutral API request observability.
/// </summary>
public static class SmartCitiesApiObservabilityExtensions
{
  /// <summary>
  /// Configures correlation/logging context and OpenTelemetry tracing with no required exporter.
  /// </summary>
  /// <param name="services">Host dependency-injection services.</param>
  /// <returns>The same service collection for composition chaining.</returns>
  /// <remarks>
  /// This overload preserves lightweight test/host composition with no external telemetry configuration.
  /// </remarks>
  public static IServiceCollection AddSmartCitiesApiObservability(
    this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    return services.AddSmartCitiesApiObservability(
      new ConfigurationBuilder().Build());
  }

  /// <summary>
  /// Configures correlation/logging context and OpenTelemetry tracing from host configuration.
  /// </summary>
  /// <param name="services">Host dependency-injection services.</param>
  /// <param name="configuration">Host configuration containing optional build and OTLP metadata.</param>
  /// <returns>The same service collection for composition chaining.</returns>
  public static IServiceCollection AddSmartCitiesApiObservability(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    var buildMetadata =
      SmartCitiesBuildMetadata.Create(configuration);
    var telemetryOptions =
      SmartCitiesOpenTelemetryOptions.Create(
        configuration);

    services.AddSingleton(buildMetadata);
    services.AddSingleton(telemetryOptions);

    services.Configure<LoggerFactoryOptions>(
      options =>
      {
        options.ActivityTrackingOptions =
          ActivityTrackingOptions.TraceId
          | ActivityTrackingOptions.SpanId
          | ActivityTrackingOptions.ParentId;
      });

    var openTelemetry = services
      .AddOpenTelemetry()
      .ConfigureResource(
        resource =>
        {
          resource.AddService(
            serviceName:
              buildMetadata.ServiceName,
            serviceVersion:
              buildMetadata.Version);

          var attributes =
            new List<KeyValuePair<string, object>>();

          if (buildMetadata.CommitSha is not null)
          {
            attributes.Add(
              new KeyValuePair<string, object>(
                "smartcities.build.commit",
                buildMetadata.CommitSha));
          }

          if (buildMetadata.BuildId is not null)
          {
            attributes.Add(
              new KeyValuePair<string, object>(
                "smartcities.build.id",
                buildMetadata.BuildId));
          }

          if (attributes.Count > 0)
          {
            resource.AddAttributes(attributes);
          }
        })
      .WithTracing(
        tracing =>
        {
          tracing.AddAspNetCoreInstrumentation();

          if (telemetryOptions.OtlpEndpoint is not null)
          {
            tracing.AddOtlpExporter(
              exporter =>
                exporter.Endpoint =
                  telemetryOptions.OtlpEndpoint);
          }
        });

    return services;
  }

  /// <summary>
  /// Adds the SmartCities correlation and structured request logging middleware.
  /// </summary>
  /// <param name="app">HTTP pipeline builder.</param>
  /// <returns>The same application builder for pipeline chaining.</returns>
  public static IApplicationBuilder UseSmartCitiesRequestObservability(
    this IApplicationBuilder app)
  {
    ArgumentNullException.ThrowIfNull(app);

    return app.UseMiddleware<
      SmartCitiesRequestObservabilityMiddleware>();
  }
}
