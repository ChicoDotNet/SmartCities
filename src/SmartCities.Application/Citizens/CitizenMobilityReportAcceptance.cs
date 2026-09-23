using SmartCities.Evidence;

namespace SmartCities.Application.Citizens;

/// <summary>
/// Represents the application-layer result of accepting a citizen mobility report.
/// </summary>
/// <remarks>
/// <see cref="WasCreated"/> distinguishes first acceptance from an idempotent replay of the same report identifier.
/// </remarks>
public sealed record CitizenMobilityReportAcceptance
{
  private CitizenMobilityReportAcceptance(
    string reportId,
    EvidenceCase evidenceCase,
    bool wasCreated)
  {
    ReportId = reportId;
    EvidenceCase = evidenceCase;
    WasCreated = wasCreated;
  }

  /// <summary>Gets the stable report identifier used as the idempotency key.</summary>
  public string ReportId { get; }

  /// <summary>Gets the authoritative Evidence Case associated with the report.</summary>
  public EvidenceCase EvidenceCase { get; }

  /// <summary>Gets a value indicating whether this acceptance created the authoritative case.</summary>
  public bool WasCreated { get; }

  /// <summary>Creates a result for the first accepted occurrence of a report.</summary>
  /// <param name="reportId">Stable report identifier.</param>
  /// <param name="evidenceCase">Authoritative Evidence Case created for the report.</param>
  /// <returns>A created acceptance result.</returns>
  public static CitizenMobilityReportAcceptance Created(
    string reportId,
    EvidenceCase evidenceCase) =>
    Create(reportId, evidenceCase, wasCreated: true);

  /// <summary>Creates a result for an idempotent replay of an already accepted report.</summary>
  /// <param name="reportId">Stable report identifier.</param>
  /// <param name="evidenceCase">Existing authoritative Evidence Case associated with the report.</param>
  /// <returns>An existing acceptance result.</returns>
  public static CitizenMobilityReportAcceptance Existing(
    string reportId,
    EvidenceCase evidenceCase) =>
    Create(reportId, evidenceCase, wasCreated: false);

  private static CitizenMobilityReportAcceptance Create(
    string reportId,
    EvidenceCase evidenceCase,
    bool wasCreated)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reportId);
    ArgumentNullException.ThrowIfNull(evidenceCase);

    return new CitizenMobilityReportAcceptance(
      reportId,
      evidenceCase,
      wasCreated);
  }
}
