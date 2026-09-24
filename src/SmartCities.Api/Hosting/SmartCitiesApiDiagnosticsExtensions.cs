using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Registers and maps the public API diagnostics surface.
/// </summary>
public static class SmartCitiesApiDiagnosticsExtensions
{
  /// <summary>
  /// Registers OpenAPI document generation for the SmartCities API.
  /// </summary>
  /// <param name="services">Host dependency-injection services.</param>
  /// <returns>The same service collection for composition chaining.</returns>
  public static IServiceCollection AddSmartCitiesApiDiagnostics(
    this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddOpenApi();

    return services;
  }

  /// <summary>
  /// Maps the OpenAPI document, process liveness, and dependency readiness endpoints.
  /// </summary>
  /// <param name="app">Built SmartCities API application.</param>
  /// <returns>The same application for route-composition chaining.</returns>
  public static WebApplication MapSmartCitiesApiDiagnostics(
    this WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);

    app.MapOpenApi();

    app.MapHealthChecks(
      "/health/live",
      new HealthCheckOptions
      {
        Predicate = static _ => false,
        ResponseWriter = WriteHealthResponseAsync,
      });

    app.MapHealthChecks(
      "/health/ready",
      new HealthCheckOptions
      {
        Predicate = static registration =>
          registration.Tags.Contains(
            "ready",
            StringComparer.Ordinal),
        ResponseWriter = WriteHealthResponseAsync,
      });

    return app;
  }

  private static Task WriteHealthResponseAsync(
    HttpContext context,
    HealthReport report)
  {
    context.Response.ContentType =
      "application/json; charset=utf-8";

    var checks = report.Entries
      .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
      .ToDictionary(
        static entry => entry.Key,
        static entry => entry.Value.Status.ToString(),
        StringComparer.Ordinal);

    return context.Response.WriteAsJsonAsync(
      new ApiHealthResponse(
        report.Status.ToString(),
        checks),
      new JsonSerializerOptions(JsonSerializerDefaults.Web),
      context.RequestAborted);
  }

  private sealed record ApiHealthResponse(
    string Status,
    IReadOnlyDictionary<string, string> Checks);
}
