namespace SmartCities.Api.Citizens;

/// <summary>
/// Represents the public-safe reviewed outcome of a citizen mobility report.
/// </summary>
/// <param name="ReportId">Stable citizen report identifier.</param>
/// <param name="CaseId">Authoritative Evidence Case identifier.</param>
/// <param name="Status">Stable machine-readable review status.</param>
/// <param name="Disposition">Stable machine-readable final human disposition, or null while pending.</param>
/// <param name="StatusLabel">Localized citizen-facing status label.</param>
/// <param name="Explanation">Localized plain-language explanation of the authoritative review state.</param>
public sealed record CitizenMobilityOutcomeResponse(
  string ReportId,
  string CaseId,
  string Status,
  string? Disposition,
  string StatusLabel,
  string Explanation);
