using SmartCities.CityContext;

namespace SmartCities.UrbanAccessibility.Routing;

/// <summary>
/// Captures provider-neutral engine/adapter/source provenance for a routing result.
/// </summary>
public sealed record AccessibilityRoutingProvenance
{
  private AccessibilityRoutingProvenance(
    string engineId,
    string? engineVersion,
    string adapterId,
    string adapterVersion,
    IReadOnlyList<CityContextProvenance> sources,
    DateTimeOffset generatedAtUtc)
  {
    EngineId = engineId;
    EngineVersion = engineVersion;
    AdapterId = adapterId;
    AdapterVersion = adapterVersion;
    Sources = sources;
    GeneratedAtUtc = generatedAtUtc;
  }

  /// <summary>Gets the stable routing-engine identifier.</summary>
  public string EngineId { get; }

  /// <summary>Gets the engine version when known.</summary>
  public string? EngineVersion { get; }

  /// <summary>Gets the stable SmartCities adapter identifier.</summary>
  public string AdapterId { get; }

  /// <summary>Gets the adapter version.</summary>
  public string AdapterVersion { get; }

  /// <summary>Gets input/source provenance used to produce the routing result.</summary>
  public IReadOnlyList<CityContextProvenance> Sources { get; }

  /// <summary>Gets the instant the result was generated, normalized to UTC.</summary>
  public DateTimeOffset GeneratedAtUtc { get; }

  /// <summary>Creates validated routing provenance.</summary>
  public static AccessibilityRoutingProvenance Create(
    string engineId,
    string? engineVersion,
    string adapterId,
    string adapterVersion,
    IEnumerable<CityContextProvenance> sources,
    DateTimeOffset generatedAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      engineId);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      adapterId);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      adapterVersion);
    ArgumentNullException.ThrowIfNull(
      sources);

    if (engineVersion is not null
        && string.IsNullOrWhiteSpace(engineVersion))
    {
      throw new ArgumentException(
        "Engine version must be non-empty when supplied.",
        nameof(engineVersion));
    }

    var sourceSnapshot =
      sources.ToArray();

    if (sourceSnapshot.Length == 0)
    {
      throw new ArgumentException(
        "Routing provenance requires at least one source.",
        nameof(sources));
    }

    if (sourceSnapshot.Any(
        static source =>
          source is null))
    {
      throw new ArgumentException(
        "Routing provenance sources cannot contain null entries.",
        nameof(sources));
    }

    return new AccessibilityRoutingProvenance(
      engineId.Trim(),
      engineVersion?.Trim(),
      adapterId.Trim(),
      adapterVersion.Trim(),
      Array.AsReadOnly(sourceSnapshot),
      generatedAt.ToUniversalTime());
  }
}
