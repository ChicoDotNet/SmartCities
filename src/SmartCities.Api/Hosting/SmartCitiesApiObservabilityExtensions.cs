using Microsoft.Extensions.Logging;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Registers and enables vendor-neutral API request observability.
/// </summary>
public static class SmartCitiesApiObservabilityExtensions
{
  /// <summary>
  /// Configures logger activity tracking for standard W3C trace metadata.
  /// </summary>
  /// <param name="services">Host dependency-injection services.</param>
  /// <returns>The same service collection for composition chaining.</returns>
  public static IServiceCollection AddSmartCitiesApiObservability(
    this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.Configure<LoggerFactoryOptions>(
      options =>
      {
        options.ActivityTrackingOptions =
          ActivityTrackingOptions.TraceId
          | ActivityTrackingOptions.SpanId
          | ActivityTrackingOptions.ParentId;
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
