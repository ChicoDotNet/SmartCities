namespace SmartCities.Api.Citizens;

/// <summary>
/// Represents the authoritative persisted case associated with a citizen mobility report.
/// </summary>
/// <param name="ReportId">Stable citizen report identifier.</param>
/// <param name="CaseId">Authoritative persisted Evidence Case identifier.</param>
public sealed record CitizenMobilityReportCaseResponse(
  string ReportId,
  string CaseId);
