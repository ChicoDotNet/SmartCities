using SmartCities.Decisions;

namespace SmartCities.Application.Citizens;

/// <summary>
/// Represents the public-safe authoritative review state associated with a citizen mobility report.
/// </summary>
/// <param name="ReportId">Stable citizen report identifier.</param>
/// <param name="CaseId">Authoritative Evidence Case identifier.</param>
/// <param name="Status">Current accountable human-review state.</param>
/// <param name="Disposition">Final human disposition when the review is finalized.</param>
public sealed record CitizenMobilityReportOutcome(
  string ReportId,
  string CaseId,
  DecisionReviewStatus Status,
  DecisionDisposition? Disposition);
