using Microsoft.Extensions.Configuration;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Represents validated vendor-neutral OpenTelemetry export configuration.
/// </summary>
public sealed record SmartCitiesOpenTelemetryOptions
{
  private SmartCitiesOpenTelemetryOptions(
    Uri? otlpEndpoint)
  {
    OtlpEndpoint = otlpEndpoint;
  }

  /// <summary>Gets the optional OTLP collector endpoint.</summary>
  public Uri? OtlpEndpoint { get; }

  internal static SmartCitiesOpenTelemetryOptions Create(
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    var configuredEndpoint =
      configuration[
        "SmartCities:Observability:Otlp:Endpoint"];

    if (string.IsNullOrWhiteSpace(configuredEndpoint))
    {
      configuredEndpoint =
        configuration[
          "OTEL_EXPORTER_OTLP_ENDPOINT"];
    }

    if (string.IsNullOrWhiteSpace(configuredEndpoint))
    {
      return new SmartCitiesOpenTelemetryOptions(
        otlpEndpoint: null);
    }

    if (!Uri.TryCreate(
        configuredEndpoint.Trim(),
        UriKind.Absolute,
        out var endpoint)
      || (endpoint.Scheme != Uri.UriSchemeHttp
        && endpoint.Scheme != Uri.UriSchemeHttps))
    {
      throw new InvalidOperationException(
        "The configured OTLP endpoint must be an absolute HTTP or HTTPS URI.");
    }

    return new SmartCitiesOpenTelemetryOptions(
      endpoint);
  }
}
