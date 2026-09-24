using SmartCities.Decisions;

namespace SmartCities.Infrastructure.Persistence.HumanOversight;

internal sealed class DecisionReviewRecord
{
  public string RecommendationId { get; set; } = string.Empty;

  public string? CriterionRequestId { get; set; }

  public string? EvidenceCaseId { get; set; }

  public string EvidenceReferenceIdsJson { get; set; } = "[]";

  public DecisionReviewStatus Status { get; set; }

  public string? AuthoritySubjectId { get; set; }

  public string? AuthorityRole { get; set; }

  public DecisionDisposition? Disposition { get; set; }
}
