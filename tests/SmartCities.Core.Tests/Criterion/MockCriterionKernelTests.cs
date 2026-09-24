using SmartCities.Criterion;
using Xunit;

namespace SmartCities.Core.Tests.Criterion;

public sealed class MockCriterionKernelTests
{
  [Fact]
  public async Task Same_request_produces_the_same_trace()
  {
    var request = CriterionDecisionRequest.Create(
      "request-001",
      "Evaluate a reported pedestrian crossing risk.",
      ["evidence-002", "evidence-001"]);

    var kernel = new MockCriterionKernel();

    var first = await kernel.EvaluateAsync(request, TestContext.Current.CancellationToken);
    var second = await kernel.EvaluateAsync(request, TestContext.Current.CancellationToken);

    Assert.Equal(first.RequestId, second.RequestId);
    Assert.Equal(first.RecommendationId, second.RecommendationId);
    Assert.Equal(first.Recommendation, second.Recommendation);
    Assert.Equal(first.PublicExplanation, second.PublicExplanation);
    Assert.Equal(first.EvidenceReferenceIds, second.EvidenceReferenceIds);
  }

  [Fact]
  public async Task Mock_trace_preserves_evidence_and_requires_human_review()
  {
    var request = CriterionDecisionRequest.Create(
      "request-002",
      "Evaluate a reported transit stop accessibility problem.",
      ["evidence-photo", "evidence-location"]);

    var trace = await new MockCriterionKernel().EvaluateAsync(request, TestContext.Current.CancellationToken);

    Assert.Equal("request-002", trace.RequestId);
    Assert.Equal(
      CriterionRecommendation.RequiresHumanReview,
      trace.Recommendation);
    Assert.True(trace.RequiresHumanReview);
    Assert.Equal(
      ["evidence-photo", "evidence-location"],
      trace.EvidenceReferenceIds);
    Assert.False(string.IsNullOrWhiteSpace(trace.PublicExplanation));
  }

  [Fact]
  public void Request_defensively_copies_evidence_references()
  {
    var evidence = new List<string> { "evidence-001" };

    var request = CriterionDecisionRequest.Create(
      "request-003",
      "Evaluate a mobility issue.",
      evidence);

    evidence.Add("evidence-002");

    Assert.Equal(["evidence-001"], request.EvidenceReferenceIds);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Request_requires_a_non_empty_request_identifier(string requestId)
  {
    Assert.Throws<ArgumentException>(
      () => CriterionDecisionRequest.Create(
        requestId,
        "Evaluate a mobility issue.",
        ["evidence-001"]));
  }
}
