using System.ComponentModel.DataAnnotations;

namespace SmartCities.Api.Citizens;

/// <summary>
/// Represents the HTTP contract for submitting a citizen mobility report.
/// </summary>
/// <param name="ReportId">Stable report identifier supplied by the client or upstream channel.</param>
/// <param name="CaseId">Candidate case identifier used only for first acceptance.</param>
/// <param name="CategoryKey">Stable non-localized mobility category key.</param>
/// <param name="LocationReference">Provider-neutral location reference.</param>
/// <param name="Description">Citizen-provided problem description.</param>
/// <param name="EvidenceReferences">Optional public-safe evidence references.</param>
public sealed record CreateCitizenMobilityReportRequest(
  [property: Required] string ReportId,
  [property: Required] string CaseId,
  [property: Required] string CategoryKey,
  [property: Required] string LocationReference,
  [property: Required] string Description,
  IReadOnlyList<CitizenEvidenceReferenceRequest>? EvidenceReferences);
