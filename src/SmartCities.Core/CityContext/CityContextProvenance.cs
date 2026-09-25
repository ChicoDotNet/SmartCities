namespace SmartCities.CityContext;

/// <summary>
/// Captures provider-neutral provenance for one City Context record.
/// </summary>
/// <remarks>
/// Source version and effective time remain optional because an upstream dataset may not expose them.
/// Retrieved time is always explicit so SmartCities can distinguish unknown freshness from missing ingestion context.
/// </remarks>
public sealed record CityContextProvenance
{
  private CityContextProvenance(
    string sourceSystem,
    string sourceReference,
    string? sourceVersion,
    DateTimeOffset? effectiveAtUtc,
    DateTimeOffset retrievedAtUtc)
  {
    SourceSystem = sourceSystem;
    SourceReference = sourceReference;
    SourceVersion = sourceVersion;
    EffectiveAtUtc = effectiveAtUtc;
    RetrievedAtUtc = retrievedAtUtc;
  }

  /// <summary>Gets the stable system or source category that supplied the record.</summary>
  public string SourceSystem { get; }

  /// <summary>Gets the source-local dataset, resource, or record reference.</summary>
  public string SourceReference { get; }

  /// <summary>Gets the source version when the upstream source exposes one.</summary>
  public string? SourceVersion { get; }

  /// <summary>Gets the source effective time in UTC when known.</summary>
  public DateTimeOffset? EffectiveAtUtc { get; }

  /// <summary>Gets the time SmartCities retrieved or received the source record, normalized to UTC.</summary>
  public DateTimeOffset RetrievedAtUtc { get; }

  /// <summary>Creates validated City Context provenance.</summary>
  public static CityContextProvenance Create(
    string sourceSystem,
    string sourceReference,
    string? sourceVersion,
    DateTimeOffset? effectiveAt,
    DateTimeOffset retrievedAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      sourceSystem);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      sourceReference);

    if (sourceVersion is not null
        && string.IsNullOrWhiteSpace(sourceVersion))
    {
      throw new ArgumentException(
        "Source version must be non-empty when supplied.",
        nameof(sourceVersion));
    }

    return new CityContextProvenance(
      sourceSystem.Trim(),
      sourceReference.Trim(),
      sourceVersion?.Trim(),
      effectiveAt?.ToUniversalTime(),
      retrievedAt.ToUniversalTime());
  }
}
