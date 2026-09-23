namespace SmartCities.Evidence;

/// <summary>
/// Represents an immutable evidence snapshot associated with a civic case or question.
/// </summary>
/// <remarks>
/// The case stores references and provenance, not binary evidence payloads. Persistence, authorization,
/// retention, and protected source material remain infrastructure concerns.
/// </remarks>
public sealed record EvidenceCase
{
  private EvidenceCase(
    string caseId,
    string subject,
    IReadOnlyList<EvidenceReference> evidenceReferences,
    IReadOnlyList<string> evidenceReferenceIds)
  {
    CaseId = caseId;
    Subject = subject;
    EvidenceReferences = evidenceReferences;
    EvidenceReferenceIds = evidenceReferenceIds;
  }

  /// <summary>Gets the stable identifier for the evidence case.</summary>
  public string CaseId { get; }

  /// <summary>Gets the bounded subject represented by the case.</summary>
  public string Subject { get; }

  /// <summary>Gets the immutable evidence-reference snapshot in caller-provided order.</summary>
  public IReadOnlyList<EvidenceReference> EvidenceReferences { get; }

  /// <summary>Gets the stable evidence identifiers in the same order as <see cref="EvidenceReferences"/>.</summary>
  public IReadOnlyList<string> EvidenceReferenceIds { get; }

  /// <summary>Creates a validated evidence-case snapshot.</summary>
  /// <param name="caseId">Stable non-empty case identifier.</param>
  /// <param name="subject">Non-empty bounded subject for the case.</param>
  /// <param name="evidenceReferences">Optional evidence references currently attached to the case.</param>
  /// <returns>An immutable case snapshot.</returns>
  /// <exception cref="ArgumentException">
  /// Thrown when required text is empty or when an evidence identifier occurs more than once.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="evidenceReferences"/> is <see langword="null"/>.
  /// </exception>
  public static EvidenceCase Create(
    string caseId,
    string subject,
    IEnumerable<EvidenceReference> evidenceReferences)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
    ArgumentException.ThrowIfNullOrWhiteSpace(subject);
    ArgumentNullException.ThrowIfNull(evidenceReferences);

    var evidence = evidenceReferences.ToArray();
    var seenIds = new HashSet<string>(StringComparer.Ordinal);

    foreach (var reference in evidence)
    {
      if (!seenIds.Add(reference.EvidenceId))
      {
        throw new ArgumentException(
          $"Evidence identifier '{reference.EvidenceId}' occurs more than once in the case.",
          nameof(evidenceReferences));
      }
    }

    var identifiers = evidence.Select(static reference => reference.EvidenceId).ToArray();

    return new EvidenceCase(
      caseId,
      subject,
      Array.AsReadOnly(evidence),
      Array.AsReadOnly(identifiers));
  }
}
