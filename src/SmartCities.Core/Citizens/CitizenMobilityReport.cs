using SmartCities.Evidence;

namespace SmartCities.Citizens;

/// <summary>
/// Represents a validated citizen-originated mobility report before application-layer persistence or routing.
/// </summary>
/// <remarks>
/// Stable keys and references are kept separate from localized display text. This core contract does not resolve
/// GIS coordinates, identity, storage, or municipal workflow.
/// </remarks>
public sealed record CitizenMobilityReport
{
  private CitizenMobilityReport(
    string reportId,
    string categoryKey,
    string locationReference,
    string description,
    IReadOnlyList<EvidenceReference> evidenceReferences,
    IReadOnlyList<string> evidenceReferenceIds)
  {
    ReportId = reportId;
    CategoryKey = categoryKey;
    LocationReference = locationReference;
    Description = description;
    EvidenceReferences = evidenceReferences;
    EvidenceReferenceIds = evidenceReferenceIds;
  }

  /// <summary>Gets the stable citizen-report identifier.</summary>
  public string ReportId { get; }

  /// <summary>Gets the stable, non-localized mobility category key.</summary>
  public string CategoryKey { get; }

  /// <summary>
  /// Gets the provider-neutral location reference supplied or resolved for the report.
  /// </summary>
  public string LocationReference { get; }

  /// <summary>Gets the citizen-provided mobility-problem description.</summary>
  public string Description { get; }

  /// <summary>Gets the immutable evidence-reference snapshot attached to the report.</summary>
  public IReadOnlyList<EvidenceReference> EvidenceReferences { get; }

  /// <summary>Gets attached evidence identifiers in stable caller-provided order.</summary>
  public IReadOnlyList<string> EvidenceReferenceIds { get; }

  /// <summary>Creates a validated citizen mobility report.</summary>
  /// <param name="reportId">Stable non-empty report identifier.</param>
  /// <param name="categoryKey">Stable non-localized category key.</param>
  /// <param name="locationReference">
  /// Non-empty provider-neutral location reference, such as an address, landmark, municipal location ID, or GIS-derived identifier.
  /// </param>
  /// <param name="description">Non-empty citizen-provided problem description.</param>
  /// <param name="evidenceReferences">Optional evidence references attached to the report.</param>
  /// <returns>An immutable report snapshot.</returns>
  /// <exception cref="ArgumentException">
  /// Thrown when required text is empty or an evidence identifier is duplicated.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="evidenceReferences"/> is <see langword="null"/>.
  /// </exception>
  public static CitizenMobilityReport Create(
    string reportId,
    string categoryKey,
    string locationReference,
    string description,
    IEnumerable<EvidenceReference> evidenceReferences)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reportId);
    ArgumentException.ThrowIfNullOrWhiteSpace(categoryKey);
    ArgumentException.ThrowIfNullOrWhiteSpace(locationReference);
    ArgumentException.ThrowIfNullOrWhiteSpace(description);
    ArgumentNullException.ThrowIfNull(evidenceReferences);

    var evidence = evidenceReferences.ToArray();
    var seenIds = new HashSet<string>(StringComparer.Ordinal);

    foreach (var reference in evidence)
    {
      ArgumentNullException.ThrowIfNull(reference);

      if (!seenIds.Add(reference.EvidenceId))
      {
        throw new ArgumentException(
          $"Evidence identifier '{reference.EvidenceId}' occurs more than once in the report.",
          nameof(evidenceReferences));
      }
    }

    var identifiers = evidence.Select(static reference => reference.EvidenceId).ToArray();

    return new CitizenMobilityReport(
      reportId,
      categoryKey,
      locationReference,
      description,
      Array.AsReadOnly(evidence),
      Array.AsReadOnly(identifiers));
  }

  /// <summary>
  /// Creates the immutable Evidence Case snapshot representing this report's bounded description and evidence.
  /// </summary>
  /// <param name="caseId">Stable non-empty case identifier allocated by the application workflow.</param>
  /// <returns>One Evidence Case snapshot for this invocation.</returns>
  /// <remarks>
  /// Exactly-once persistence and case-ID allocation belong to the later application service and Unit of Work boundary.
  /// This method is deterministic and performs no I/O.
  /// </remarks>
  public EvidenceCase CreateEvidenceCase(string caseId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(caseId);

    return EvidenceCase.Create(
      caseId,
      Description,
      EvidenceReferences);
  }
}
