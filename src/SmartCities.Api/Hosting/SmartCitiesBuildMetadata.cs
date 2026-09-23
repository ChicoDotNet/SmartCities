using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Describes the public build identity of the running SmartCities API.
/// </summary>
public sealed record SmartCitiesBuildMetadata
{
  private const int MaxPublicBuildIdentityLength = 128;

  private SmartCitiesBuildMetadata(
    string serviceName,
    string version,
    string informationalVersion,
    string? commitSha,
    string? buildId)
  {
    ServiceName = serviceName;
    Version = version;
    InformationalVersion = informationalVersion;
    CommitSha = commitSha;
    BuildId = buildId;
  }

  /// <summary>Gets the stable OpenTelemetry/API service name.</summary>
  public string ServiceName { get; }

  /// <summary>Gets the assembly version of the running API.</summary>
  public string Version { get; }

  /// <summary>Gets the assembly informational version of the running API.</summary>
  public string InformationalVersion { get; }

  /// <summary>Gets the optional source commit configured by the build/deployment pipeline.</summary>
  public string? CommitSha { get; }

  /// <summary>Gets the optional build identifier configured by the build/deployment pipeline.</summary>
  public string? BuildId { get; }

  internal static SmartCitiesBuildMetadata Create(
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    var assembly =
      typeof(SmartCitiesBuildMetadata).Assembly;
    var assemblyName = assembly.GetName();
    var serviceName =
      assemblyName.Name
      ?? "SmartCities.Api";
    var version =
      assemblyName.Version?.ToString()
      ?? "0.0.0";
    var informationalVersion =
      assembly
        .GetCustomAttribute<
          AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion
      ?? version;

    var configuredCommit = NormalizePublicValue(
      configuration["SmartCities:Build:CommitSha"],
      "SmartCities:Build:CommitSha");
    var buildId = NormalizePublicValue(
      configuration["SmartCities:Build:BuildId"],
      "SmartCities:Build:BuildId");

    return new SmartCitiesBuildMetadata(
      serviceName,
      version,
      informationalVersion,
      configuredCommit
        ?? TryExtractCommit(informationalVersion),
      buildId);
  }

  private static string? NormalizePublicValue(
    string? value,
    string key)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      return null;
    }

    var normalized = value.Trim();

    if (normalized.Length > MaxPublicBuildIdentityLength
      || normalized.Any(
        static character =>
          char.IsControl(character)))
    {
      throw new InvalidOperationException(
        $"Configuration value '{key}' is not a valid public build identifier.");
    }

    return normalized;
  }

  private static string? TryExtractCommit(
    string informationalVersion)
  {
    var separator = informationalVersion.LastIndexOf(
      '+');

    if (separator < 0
      || separator == informationalVersion.Length - 1)
    {
      return null;
    }

    var candidate =
      informationalVersion[(separator + 1)..];

    return candidate.Length is >= 7 and <= 64
      && candidate.All(
        static character =>
          char.IsAsciiHexDigit(character))
        ? candidate
        : null;
  }
}
