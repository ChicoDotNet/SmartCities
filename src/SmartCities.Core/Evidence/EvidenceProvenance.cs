namespace SmartCities.Evidence;

/// <summary>
/// Captures public-safe provenance metadata for a referenced evidence item.
/// </summary>
public sealed record EvidenceProvenance
{
  private EvidenceProvenance(
    string sourceSystem,
    string sourceReference,
    DateTimeOffset observedAtUtc)
  {
    SourceSystem = sourceSystem;
    SourceReference = sourceReference;
    ObservedAtUtc = observedAtUtc;
  }

  /// <summary>Gets the stable system, channel, or source category that supplied the evidence.</summary>
  public string SourceSystem { get; }

  /// <summary>Gets the source-local reference used to trace the evidence back to its origin.</summary>
  public string SourceReference { get; }

  /// <summary>Gets the observation timestamp normalized to UTC.</summary>
  public DateTimeOffset ObservedAtUtc { get; }

  /// <summary>Creates validated public-safe provenance metadata.</summary>
  /// <param name="sourceSystem">Stable non-empty source-system identifier.</param>
  /// <param name="sourceReference">Stable non-empty reference within that source.</param>
  /// <param name="observedAt">Timestamp associated with the source observation.</param>
  /// <returns>Immutable provenance metadata with a UTC-normalized timestamp.</returns>
  /// <exception cref="ArgumentException">
  /// Thrown when <paramref name="sourceSystem"/> or <paramref name="sourceReference"/> is empty or whitespace.
  /// </exception>
  public static EvidenceProvenance Create(
    string sourceSystem,
    string sourceReference,
    DateTimeOffset observedAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(sourceSystem);
    ArgumentException.ThrowIfNullOrWhiteSpace(sourceReference);

    return new EvidenceProvenance(
      sourceSystem,
      sourceReference,
      observedAt.ToUniversalTime());
  }
}
