namespace SmartCities.Application.Citizens;

/// <summary>
/// Defines the citizen-facing query for the authoritative reviewed outcome of a mobility report.
/// </summary>
public interface ICitizenMobilityOutcomeService
{
  /// <summary>
  /// Gets the authoritative human-review outcome for a report without exposing internal review identities.
  /// </summary>
  /// <param name="reportId">Stable citizen report identifier.</param>
  /// <param name="cancellationToken">Token used to cancel the operation.</param>
  /// <returns>The public-safe outcome, or <see langword="null"/> when the report does not exist.</returns>
  Task<CitizenMobilityReportOutcome?> GetAsync(
    string reportId,
    CancellationToken cancellationToken = default);
}
