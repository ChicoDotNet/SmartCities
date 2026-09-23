namespace SmartCities.Evidence;

/// <summary>
/// Represents a stable evidence identity and its public-safe provenance without embedding evidence payloads.
/// </summary>
public sealed record EvidenceReference
{
  private EvidenceReference(
    string evidenceId,
    EvidenceKind kind,
    EvidenceProvenance provenance)
  {
    EvidenceId = evidenceId;
    Kind = kind;
    Provenance = provenance;
  }

  /// <summary>Gets the stable SmartCities identifier for the referenced evidence.</summary>
  public string EvidenceId { get; }

  /// <summary>Gets the public evidence classification.</summary>
  public EvidenceKind Kind { get; }

  /// <summary>Gets the provenance required to trace the evidence to its source.</summary>
  public EvidenceProvenance Provenance { get; }

  /// <summary>Creates a validated evidence reference.</summary>
  /// <param name="evidenceId">Stable non-empty evidence identifier.</param>
  /// <param name="kind">Public evidence classification.</param>
  /// <param name="provenance">Public-safe source provenance.</param>
  /// <returns>An immutable evidence reference.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="evidenceId"/> is empty or whitespace.</exception>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="provenance"/> is <see langword="null"/>.</exception>
  public static EvidenceReference Create(
    string evidenceId,
    EvidenceKind kind,
    EvidenceProvenance provenance)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(evidenceId);
    ArgumentNullException.ThrowIfNull(provenance);

    return new EvidenceReference(evidenceId, kind, provenance);
  }
}
