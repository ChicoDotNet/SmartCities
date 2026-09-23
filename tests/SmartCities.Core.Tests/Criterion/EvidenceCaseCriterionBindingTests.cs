using SmartCities.Criterion;
using SmartCities.Evidence;
using Xunit;

namespace SmartCities.Core.Tests.Criterion;

public sealed class EvidenceCaseCriterionBindingTests
{
  [Fact]
  public void Request_from_evidence_case_preserves_case_identity_subject_and_evidence_order()
  {
    var evidenceCase = EvidenceCase.Create(
      "case-001",
      "Unsafe pedestrian crossing",
      [
        CreateEvidence("evidence-002"),
        CreateEvidence("evidence-001"),
      ]);

    var request = CriterionDecisionRequest.FromEvidenceCase(
      "request-001",
      evidenceCase);

    Assert.Equal("request-001", request.RequestId);
    Assert.Equal("case-001", request.EvidenceCaseId);
    Assert.Equal("Unsafe pedestrian crossing", request.Subject);
    Assert.Equal(
      ["evidence-002", "evidence-001"],
      request.EvidenceReferenceIds);
  }

  [Fact]
  public async Task Mock_trace_preserves_the_evidence_case_link()
  {
    var evidenceCase = EvidenceCase.Create(
      "case-002",
      "Transit stop accessibility problem",
      [CreateEvidence("evidence-photo")]);

    var request = CriterionDecisionRequest.FromEvidenceCase(
      "request-002",
      evidenceCase);

    var trace = await new MockCriterionKernel().EvaluateAsync(
      request,
      TestContext.Current.CancellationToken);

    Assert.Equal("case-002", trace.EvidenceCaseId);
    Assert.Equal(request.EvidenceReferenceIds, trace.EvidenceReferenceIds);
  }

  [Fact]
  public void Generic_request_remains_supported_without_an_evidence_case()
  {
    var request = CriterionDecisionRequest.Create(
      "request-003",
      "Evaluate a citywide scenario.",
      ["evidence-model"]);

    Assert.Null(request.EvidenceCaseId);
  }

  [Fact]
  public void Request_from_evidence_case_requires_a_case()
  {
    Assert.Throws<ArgumentNullException>(
      () => CriterionDecisionRequest.FromEvidenceCase(
        "request-004",
        null!));
  }

  private static EvidenceReference CreateEvidence(string evidenceId) =>
    EvidenceReference.Create(
      evidenceId,
      EvidenceKind.CitizenStatement,
      EvidenceProvenance.Create(
        "citizen-portal",
        $"submission:{evidenceId}",
        new DateTimeOffset(2026, 9, 23, 16, 45, 0, TimeSpan.Zero)));
}
