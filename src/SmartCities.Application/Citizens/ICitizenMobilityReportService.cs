using SmartCities.Citizens;

namespace SmartCities.Application.Citizens;

/// <summary>
/// Defines application use cases for accepting and retrieving citizen mobility reports.
/// </summary>
public interface ICitizenMobilityReportService
{
  /// <summary>Gets the authoritative persisted case for a report.</summary>
  /// <param name="reportId">Stable report identifier.</param>
  /// <param name="cancellationToken">Token used to cancel the operation.</param>
  /// <returns>The authoritative report/case association, or <see langword="null"/> when no report exists.</returns>
  Task<CitizenMobilityReportCase?> GetAsync(
    string reportId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Accepts a validated citizen report and returns its authoritative Evidence Case association.
  /// </summary>
  /// <param name="report">Validated domain report.</param>
  /// <param name="caseId">Candidate case identifier used only if this is the first accepted occurrence.</param>
  /// <param name="cancellationToken">Token used to cancel the operation.</param>
  /// <returns>An idempotent application acceptance result.</returns>
  Task<CitizenMobilityReportAcceptance> AcceptAsync(
    CitizenMobilityReport report,
    string caseId,
    CancellationToken cancellationToken = default);
}
