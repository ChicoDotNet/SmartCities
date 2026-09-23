namespace SmartCities.Api.Citizens;

/// <summary>
/// Represents the HTTP response after a citizen mobility report is accepted or replayed idempotently.
/// </summary>
/// <param name="ReportId">Stable accepted report identifier.</param>
/// <param name="CaseId">Authoritative Evidence Case identifier.</param>
/// <param name="WasCreated">Whether this request created the authoritative case.</param>
public sealed record CitizenMobilityReportAcceptanceResponse(
  string ReportId,
  string CaseId,
  bool WasCreated);
