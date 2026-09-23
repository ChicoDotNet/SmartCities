using System.ComponentModel.DataAnnotations;
using SmartCities.Evidence;

namespace SmartCities.Api.Citizens;

/// <summary>
/// Represents a public-safe evidence reference supplied with a citizen mobility report.
/// </summary>
/// <param name="EvidenceId">Stable client or upstream evidence identifier.</param>
/// <param name="Kind">Public evidence classification.</param>
/// <param name="SourceSystem">Public-safe source-system identifier.</param>
/// <param name="SourceReference">Public-safe source reference within that system.</param>
/// <param name="ObservedAt">Time at which the evidence was observed.</param>
public sealed record CitizenEvidenceReferenceRequest(
  [property: Required] string EvidenceId,
  [property: EnumDataType(typeof(EvidenceKind))] EvidenceKind Kind,
  [property: Required] string SourceSystem,
  [property: Required] string SourceReference,
  DateTimeOffset ObservedAt);
