using SmartCities.Decisions;

namespace SmartCities.Api.HumanOversight;

/// <summary>
/// Represents the authoritative public snapshot of a human decision review.
/// </summary>
public sealed record DecisionReviewResponse(
  string RecommendationId,
  string? CriterionRequestId,
  string? EvidenceCaseId,
  IReadOnlyList<string> EvidenceReferenceIds,
  DecisionReviewStatus Status,
  string? AuthoritySubjectId,
  string? AuthorityRole,
  DecisionDisposition? Disposition)
{
  internal static DecisionReviewResponse FromDomain(
    DecisionReview review)
  {
    ArgumentNullException.ThrowIfNull(review);

    return new DecisionReviewResponse(
      review.RecommendationId,
      review.CriterionRequestId,
      review.EvidenceCaseId,
      review.EvidenceReferenceIds,
      review.Status,
      review.Authority?.SubjectId,
      review.Authority?.Role,
      review.Disposition);
  }
}
