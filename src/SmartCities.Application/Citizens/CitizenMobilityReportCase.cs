using SmartCities.Evidence;

namespace SmartCities.Application.Citizens;

/// <summary>
/// Represents the authoritative persisted Evidence Case associated with a citizen mobility report.
/// </summary>
public sealed record CitizenMobilityReportCase
{
  private CitizenMobilityReportCase(
    string reportId,
    EvidenceCase evidenceCase)
  {
    ReportId = reportId;
    EvidenceCase = evidenceCase;
  }

  /// <summary>Gets the stable citizen report identifier.</summary>
  public string ReportId { get; }

  /// <summary>Gets the authoritative persisted Evidence Case.</summary>
  public EvidenceCase EvidenceCase { get; }

  /// <summary>Creates a validated report-to-case association.</summary>
  /// <param name="reportId">Stable citizen report identifier.</param>
  /// <param name="evidenceCase">Authoritative Evidence Case.</param>
  /// <returns>An immutable report-to-case association.</returns>
  public static CitizenMobilityReportCase Create(
    string reportId,
    EvidenceCase evidenceCase)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reportId);
    ArgumentNullException.ThrowIfNull(evidenceCase);

    return new CitizenMobilityReportCase(
      reportId,
      evidenceCase);
  }
}
