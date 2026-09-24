using SmartCities.Criterion;
using SmartCities.Decisions;
using Xunit;

namespace SmartCities.Core.Tests.Decisions;

public sealed class DecisionReviewTraceabilityTests
{
  [Fact]
  public void Pending_review_from_trace_preserves_case_request_recommendation_and_evidence_links()
  {
    var trace = CreateTrace(
      "request-001",
      "case-001",
      "recommendation-001",
      ["evidence-002", "evidence-001"]);

    var review = DecisionReview.Pending(trace);

    Assert.Equal("case-001", review.EvidenceCaseId);
    Assert.Equal("request-001", review.CriterionRequestId);
    Assert.Equal("recommendation-001", review.RecommendationId);
    Assert.Equal(
      ["evidence-002", "evidence-001"],
      review.EvidenceReferenceIds);
    Assert.Equal(
      DecisionReviewStatus.PendingHumanReview,
      review.Status);
  }

  [Fact]
  public void Finalized_review_retains_the_complete_origin_trace()
  {
    var trace = CreateTrace(
      "request-002",
      "case-002",
      "recommendation-002",
      ["evidence-photo", "evidence-location"]);
    var authority = HumanAuthority.Create(
      "reviewer-001",
      "Mobility authority");

    var finalized = DecisionReview
      .Pending(trace)
      .Finalize(authority, DecisionDisposition.Modified);

    Assert.Equal("case-002", finalized.EvidenceCaseId);
    Assert.Equal("request-002", finalized.CriterionRequestId);
    Assert.Equal("recommendation-002", finalized.RecommendationId);
    Assert.Equal(
      ["evidence-photo", "evidence-location"],
      finalized.EvidenceReferenceIds);
    Assert.Equal(authority, finalized.Authority);
    Assert.Equal(DecisionDisposition.Modified, finalized.Disposition);
    Assert.Equal(DecisionReviewStatus.Finalized, finalized.Status);
  }

  [Fact]
  public void Pending_review_rejects_a_trace_that_does_not_require_human_review()
  {
    var trace = CriterionDecisionTrace.Create(
      "request-003",
      "recommendation-003",
      CriterionRecommendation.RequiresHumanReview,
      requiresHumanReview: false,
      ["evidence-001"],
      "Invalid test trace.",
      evidenceCaseId: "case-003");

    Assert.Throws<InvalidOperationException>(
      () => DecisionReview.Pending(trace));
  }

  private static CriterionDecisionTrace CreateTrace(
    string requestId,
    string caseId,
    string recommendationId,
    IEnumerable<string> evidenceReferenceIds) =>
    CriterionDecisionTrace.Create(
      requestId,
      recommendationId,
      CriterionRecommendation.RequiresHumanReview,
      requiresHumanReview: true,
      evidenceReferenceIds,
      "Public-safe test explanation.",
      evidenceCaseId: caseId);
}
