using SmartCities.Application.Citizens;
using SmartCities.Application.HumanOversight;
using SmartCities.Criterion;
using SmartCities.Decisions;
using SmartCities.Evidence;
using Xunit;

namespace SmartCities.Application.Tests.Citizens;

public sealed class CitizenMobilityDecisionPipelineTests
{
  [Fact]
  public async Task Pipeline_builds_a_deterministic_trace_from_the_authoritative_case()
  {
    var kernel = new RecordingCriterionKernel();
    var reviews = new RecordingDecisionReviewRepository();
    var pipeline = new CitizenMobilityDecisionPipeline(
      kernel,
      reviews);
    var evidenceCase = EvidenceCase.Create(
      "case-001",
      "Unsafe pedestrian crossing.",
      [
        EvidenceReference.Create(
          "evidence-001",
          EvidenceKind.CitizenStatement,
          EvidenceProvenance.Create(
            "citizen-portal",
            "submission:report-001",
            new DateTimeOffset(
              2026,
              9,
              24,
              0,
              0,
              0,
              TimeSpan.Zero))),
      ]);

    var review = await pipeline.EnsureReviewAsync(
      evidenceCase,
      TestContext.Current.CancellationToken);

    Assert.NotNull(kernel.LastRequest);
    Assert.Equal(
      "mobility:E1AD9BF98D3AF729393E416CA84E9081BD30C2FA5B704ECF0AAA2D0C77245885",
      kernel.LastRequest.RequestId);
    Assert.Equal("case-001", kernel.LastRequest.EvidenceCaseId);
    Assert.Equal(
      ["evidence-001"],
      kernel.LastRequest.EvidenceReferenceIds);
    Assert.Equal(
      $"test:{kernel.LastRequest.RequestId}",
      review.RecommendationId);
    Assert.Equal("case-001", review.EvidenceCaseId);
    Assert.Equal(
      DecisionReviewStatus.PendingHumanReview,
      review.Status);
  }

  [Fact]
  public async Task Pipeline_fails_closed_when_the_criterion_trace_breaks_the_case_link()
  {
    var pipeline = new CitizenMobilityDecisionPipeline(
      new BrokenCriterionKernel(),
      new RecordingDecisionReviewRepository());

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
      () => pipeline.EnsureReviewAsync(
        EvidenceCase.Create(
          "case-broken",
          "Broken link test.",
          []),
        TestContext.Current.CancellationToken));

    Assert.Contains(
      "preserve",
      exception.Message,
      StringComparison.Ordinal);
  }

  private sealed class RecordingCriterionKernel
    : ICriterionKernel
  {
    public CriterionDecisionRequest? LastRequest { get; private set; }

    public Task<CriterionDecisionTrace> EvaluateAsync(
      CriterionDecisionRequest request,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      LastRequest = request;

      return Task.FromResult(
        CriterionDecisionTrace.Create(
          request.RequestId,
          $"test:{request.RequestId}",
          CriterionRecommendation.RequiresHumanReview,
          requiresHumanReview: true,
          request.EvidenceReferenceIds,
          "Public test explanation.",
          request.EvidenceCaseId));
    }
  }

  private sealed class BrokenCriterionKernel
    : ICriterionKernel
  {
    public Task<CriterionDecisionTrace> EvaluateAsync(
      CriterionDecisionRequest request,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      return Task.FromResult(
        CriterionDecisionTrace.Create(
          request.RequestId,
          $"broken:{request.RequestId}",
          CriterionRecommendation.RequiresHumanReview,
          requiresHumanReview: true,
          request.EvidenceReferenceIds,
          "Broken test explanation.",
          evidenceCaseId: "different-case"));
    }
  }

  private sealed class RecordingDecisionReviewRepository
    : IDecisionReviewRepository
  {
    public Task AddPendingAsync(
      DecisionReview review,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<DecisionReview> GetOrAddPendingAsync(
      DecisionReview review,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(review);
    }

    public Task<DecisionReview?> GetAsync(
      string recommendationId,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<DecisionReviewFinalizationResult> FinalizeAsync(
      string recommendationId,
      HumanAuthority authority,
      DecisionDisposition disposition,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();
  }
}
