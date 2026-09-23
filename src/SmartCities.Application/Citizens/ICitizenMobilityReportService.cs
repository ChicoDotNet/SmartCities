using SmartCities.Citizens;

namespace SmartCities.Application.Citizens;

/// <summary>
/// Defines the application use case for accepting a validated citizen mobility report.
/// </summary>
public interface ICitizenMobilityReportService
{
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
