using System.ComponentModel.DataAnnotations;
using SmartCities.Decisions;

namespace SmartCities.Api.HumanOversight;

/// <summary>
/// Represents an accountable request to finalize a pending civic decision review.
/// </summary>
/// <param name="AuthorityRole">Canonical authority role under which the human acts.</param>
/// <param name="Disposition">Final human disposition.</param>
public sealed record FinalizeDecisionReviewRequest(
  [param: Required] string AuthorityRole,
  [param: EnumDataType(typeof(DecisionDisposition))]
  DecisionDisposition Disposition);
