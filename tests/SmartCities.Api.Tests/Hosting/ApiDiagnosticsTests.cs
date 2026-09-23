using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartCities.Api.Citizens;
using SmartCities.Api.Hosting;
using Xunit;

namespace SmartCities.Api.Tests.Hosting;

public sealed class ApiDiagnosticsTests
{
  [Fact]
  public async Task OpenApi_document_exposes_the_public_citizen_routes()
  {
    await using var app = await StartAsync(
      HealthStatus.Healthy);

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/openapi/v1.json",
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    await using var stream = await response.Content.ReadAsStreamAsync(
      TestContext.Current.CancellationToken);
    using var document = await JsonDocument.ParseAsync(
      stream,
      cancellationToken: TestContext.Current.CancellationToken);

    var paths = document.RootElement
      .GetProperty("paths")
      .EnumerateObject()
      .Select(static path => path.Name)
      .ToArray();

    Assert.Contains(
      "/api/citizen/mobility-reports",
      paths);
    Assert.Contains(
      "/api/citizen/mobility-reports/{reportId}",
      paths);
    Assert.Contains(
      paths,
      path => path.StartsWith(
        "/api/localization/resources",
        StringComparison.Ordinal));
    Assert.Contains(
      "/api/system/build",
      paths);
  }

  [Fact]
  public async Task Liveness_is_healthy_even_when_the_database_is_not_ready()
  {
    await using var app = await StartAsync(
      HealthStatus.Unhealthy);

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/health/live",
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var payload = await response.Content.ReadAsStringAsync(
      TestContext.Current.CancellationToken);

    Assert.Contains(
      "\"status\":\"Healthy\"",
      payload,
      StringComparison.Ordinal);
    Assert.DoesNotContain(
      "\"database\"",
      payload,
      StringComparison.Ordinal);
  }

  [Fact]
  public async Task Readiness_reflects_the_database_health_check()
  {
    await using var app = await StartAsync(
      HealthStatus.Unhealthy);

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/health/ready",
      TestContext.Current.CancellationToken);

    Assert.Equal(
      HttpStatusCode.ServiceUnavailable,
      response.StatusCode);

    var payload = await response.Content.ReadAsStringAsync(
      TestContext.Current.CancellationToken);

    Assert.Contains(
      "\"status\":\"Unhealthy\"",
      payload,
      StringComparison.Ordinal);
    Assert.Contains(
      "\"database\":\"Unhealthy\"",
      payload,
      StringComparison.Ordinal);
  }

  private static async Task<WebApplication> StartAsync(
    HealthStatus databaseStatus)
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();

    builder.Services
      .AddSmartCitiesApiControllers()
      .AddApplicationPart(
        typeof(CitizenMobilityReportsController).Assembly);
    builder.Services.AddSmartCitiesApiDiagnostics();
    builder.Services.AddSmartCitiesApiObservability();
    builder.Services
      .AddHealthChecks()
      .AddCheck(
        "database",
        () => new HealthCheckResult(databaseStatus),
        tags: ["ready"]);

    var app = builder.Build();

    app.MapControllers();
    app.MapSmartCitiesApiDiagnostics();
    app.MapSmartCitiesBuildMetadata();

    await app.StartAsync(
      TestContext.Current.CancellationToken);

    return app;
  }
}
