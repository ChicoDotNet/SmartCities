using SmartCities.Citizens;
using SmartCities.Criterion;
using SmartCities.Decisions;
using SmartCities.Evidence;
using SmartCities.Workflows;
using Xunit;

namespace SmartCities.Core.Tests.Workflows;

public sealed class CitizenMobilityDecisionWorkflowTests
{
  [Fact]
  public async Task Valid_report_flows_to_pending_human_review_with_full_traceability()
  {
    var report = CitizenMobilityReport.Create(
      "report-001",
      "pedestrian-safety",
      "Av. Universidad y Sierra Madre",
      "The pedestrian crossing feels unsafe at night.",
      [
        CreateEvidence("evidence-photo", EvidenceKind.Photograph),
        CreateEvidence("evidence-statement", EvidenceKind.CitizenStatement),
      ]);

    var snapshot = await CitizenMobilityDecisionWorkflow.EvaluateAsync(
      report,
      "case-001",
      "request-001",
      new MockCriterionKernel(),
      TestContext.Current.CancellationToken);

    Assert.Same(report, snapshot.Report);
    Assert.Equal("case-001", snapshot.EvidenceCase.CaseId);
    Assert.Equal("case-001", snapshot.CriterionRequest.EvidenceCaseId);
    Assert.Equal("case-001", snapshot.CriterionTrace.EvidenceCaseId);
    Assert.Equal(
      ["evidence-photo", "evidence-statement"],
      snapshot.CriterionTrace.EvidenceReferenceIds);
    Assert.Equal(
      snapshot.CriterionTrace.RecommendationId,
      snapshot.HumanReview.RecommendationId);
    Assert.Equal(
      DecisionReviewStatus.PendingHumanReview,
      snapshot.HumanReview.Status);
    Assert.Null(snapshot.HumanReview.Authority);
    Assert.Null(snapshot.HumanReview.Disposition);
  }

  [Fact]
  public async Task Workflow_rejects_a_trace_that_breaks_the_case_link()
  {
    var report = CitizenMobilityReport.Create(
      "report-002",
      "public-transport",
      "Stop AGS-1024",
      "The stop has no accessible boarding path.",
      [CreateEvidence("evidence-001", EvidenceKind.CitizenStatement)]);

    await Assert.ThrowsAsync<InvalidOperationException>(
      () => CitizenMobilityDecisionWorkflow.EvaluateAsync(
        report,
        "case-002",
        "request-002",
        new BrokenCaseLinkKernel(),
        TestContext.Current.CancellationToken));
  }

  private static EvidenceReference CreateEvidence(
    string evidenceId,
    EvidenceKind kind) =>
    EvidenceReference.Create(
      evidenceId,
      kind,
      EvidenceProvenance.Create(
        "citizen-portal",
        $"submission:{evidenceId}",
        new DateTimeOffset(2026, 9, 23, 17, 15, 0, TimeSpan.Zero)));

  private sealed class BrokenCaseLinkKernel : ICriterionKernel
  {
    public Task<CriterionDecisionTrace> EvaluateAsync(
      CriterionDecisionRequest request,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      return Task.FromResult(
        CriterionDecisionTrace.Create(
          request.RequestId,
          "broken:recommendation",
          CriterionRecommendation.RequiresHumanReview,
          requiresHumanReview: true,
          request.EvidenceReferenceIds,
          "Broken test trace.",
          evidenceCaseId: "different-case"));
    }
  }
}
