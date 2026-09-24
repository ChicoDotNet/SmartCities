using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using SmartCities.Api.Hosting;
using Xunit;

namespace SmartCities.Api.Tests.Hosting;

public sealed class BuildMetadataAndOpenTelemetryTests
{
  [Fact]
  public void Build_metadata_exposes_service_version_and_configured_source_identity()
  {
    var configuration = BuildConfiguration(
      commitSha: "abcdef1234567890",
      buildId: "ci-4242");
    var services = new ServiceCollection();

    services.AddSmartCitiesApiObservability(
      configuration);

    using var provider = services.BuildServiceProvider();
    var metadata = provider.GetRequiredService<
      SmartCitiesBuildMetadata>();

    Assert.Equal("SmartCities.Api", metadata.ServiceName);
    Assert.False(string.IsNullOrWhiteSpace(metadata.Version));
    Assert.False(
      string.IsNullOrWhiteSpace(
        metadata.InformationalVersion));
    Assert.Equal(
      "abcdef1234567890",
      metadata.CommitSha);
    Assert.Equal(
      "ci-4242",
      metadata.BuildId);
  }

  [Fact]
  public void OpenTelemetry_is_registered_without_requiring_an_exporter()
  {
    var configuration = BuildConfiguration();
    var services = new ServiceCollection();

    services.AddSmartCitiesApiObservability(
      configuration);

    using var provider = services.BuildServiceProvider();

    Assert.NotNull(
      provider.GetService<TracerProvider>());

    var options = provider.GetRequiredService<
      SmartCitiesOpenTelemetryOptions>();

    Assert.Null(options.OtlpEndpoint);
  }

  [Theory]
  [InlineData("ftp://collector.example")]
  [InlineData("not-a-uri")]
  public void Invalid_otlp_endpoint_fails_during_composition(
    string endpoint)
  {
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(
        new Dictionary<string, string?>
        {
          ["SmartCities:Observability:Otlp:Endpoint"] =
            endpoint,
        })
      .Build();
    var services = new ServiceCollection();

    var exception = Assert.Throws<InvalidOperationException>(
      () => services.AddSmartCitiesApiObservability(
        configuration));

    Assert.Contains(
      "OTLP",
      exception.Message,
      StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Standard_otlp_environment_key_is_supported()
  {
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(
        new Dictionary<string, string?>
        {
          ["OTEL_EXPORTER_OTLP_ENDPOINT"] =
            "http://collector:4317",
        })
      .Build();
    var services = new ServiceCollection();

    services.AddSmartCitiesApiObservability(
      configuration);

    using var provider = services.BuildServiceProvider();
    var options = provider.GetRequiredService<
      SmartCitiesOpenTelemetryOptions>();

    Assert.Equal(
      new Uri("http://collector:4317"),
      options.OtlpEndpoint);
  }

  [Fact]
  public async Task Build_metadata_endpoint_returns_public_deployment_identity()
  {
    var configuration = BuildConfiguration(
      commitSha: "abcdef1234567890",
      buildId: "ci-4242");
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();
    builder.Services.AddSmartCitiesApiObservability(
      configuration);

    await using var app = builder.Build();
    app.MapSmartCitiesBuildMetadata();

    await app.StartAsync(
      TestContext.Current.CancellationToken);

    using var client = app.GetTestClient();
    using var response = await client.GetAsync(
      "/api/system/build",
      TestContext.Current.CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    await using var stream = await response.Content.ReadAsStreamAsync(
      TestContext.Current.CancellationToken);
    using var document = await JsonDocument.ParseAsync(
      stream,
      cancellationToken:
        TestContext.Current.CancellationToken);

    var root = document.RootElement;

    Assert.Equal(
      "SmartCities.Api",
      root.GetProperty("serviceName").GetString());
    Assert.False(
      string.IsNullOrWhiteSpace(
        root.GetProperty("version").GetString()));
    Assert.Equal(
      "abcdef1234567890",
      root.GetProperty("commitSha").GetString());
    Assert.Equal(
      "ci-4242",
      root.GetProperty("buildId").GetString());
  }

  private static IConfiguration BuildConfiguration(
    string? commitSha = null,
    string? buildId = null)
  {
    var values = new Dictionary<string, string?>();

    if (commitSha is not null)
    {
      values["SmartCities:Build:CommitSha"] =
        commitSha;
    }

    if (buildId is not null)
    {
      values["SmartCities:Build:BuildId"] =
        buildId;
    }

    return new ConfigurationBuilder()
      .AddInMemoryCollection(values)
      .Build();
  }
}
