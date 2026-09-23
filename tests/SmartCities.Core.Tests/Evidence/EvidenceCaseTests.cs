using SmartCities.Evidence;
using Xunit;

namespace SmartCities.Core.Tests.Evidence;

public sealed class EvidenceCaseTests
{
  [Fact]
  public void Evidence_reference_preserves_provenance_metadata()
  {
    var observedAt = new DateTimeOffset(2026, 9, 23, 10, 30, 0, TimeSpan.FromHours(-6));
    var provenance = EvidenceProvenance.Create(
      "citizen-portal",
      "submission-001",
      observedAt);
    var evidence = EvidenceReference.Create(
      "evidence-001",
      EvidenceKind.Photograph,
      provenance);

    var evidenceCase = EvidenceCase.Create(
      "case-001",
      "Unsafe pedestrian crossing",
      [evidence]);

    var stored = Assert.Single(evidenceCase.EvidenceReferences);
    Assert.Equal("evidence-001", stored.EvidenceId);
    Assert.Equal(EvidenceKind.Photograph, stored.Kind);
    Assert.Equal("citizen-portal", stored.Provenance.SourceSystem);
    Assert.Equal("submission-001", stored.Provenance.SourceReference);
    Assert.Equal(observedAt.ToUniversalTime(), stored.Provenance.ObservedAtUtc);
  }

  [Fact]
  public void Evidence_case_exposes_stable_evidence_identifiers_in_input_order()
  {
    var first = CreateEvidence("evidence-002");
    var second = CreateEvidence("evidence-001");

    var evidenceCase = EvidenceCase.Create(
      "case-002",
      "Transit stop accessibility problem",
      [first, second]);

    Assert.Equal(
      ["evidence-002", "evidence-001"],
      evidenceCase.EvidenceReferenceIds);
  }

  [Fact]
  public void Evidence_case_rejects_duplicate_evidence_identifiers()
  {
    var first = CreateEvidence("evidence-001");
    var duplicate = EvidenceReference.Create(
      "evidence-001",
      EvidenceKind.Document,
      EvidenceProvenance.Create(
        "municipal-system",
        "document-099",
        new DateTimeOffset(2026, 9, 23, 16, 45, 0, TimeSpan.Zero)));

    Assert.Throws<ArgumentException>(
      () => EvidenceCase.Create(
        "case-003",
        "Repeated evidence identifier",
        [first, duplicate]));
  }

  [Fact]
  public void Evidence_case_defensively_copies_the_evidence_collection()
  {
    var evidence = new List<EvidenceReference>
    {
      CreateEvidence("evidence-001"),
    };

    var evidenceCase = EvidenceCase.Create(
      "case-004",
      "Mobility issue",
      evidence);

    evidence.Add(CreateEvidence("evidence-002"));

    Assert.Single(evidenceCase.EvidenceReferences);
    Assert.Equal(["evidence-001"], evidenceCase.EvidenceReferenceIds);
  }

  [Fact]
  public void Evidence_case_can_start_without_attached_evidence()
  {
    var evidenceCase = EvidenceCase.Create(
      "case-005",
      "Citizen report awaiting supporting evidence",
      []);

    Assert.Empty(evidenceCase.EvidenceReferences);
    Assert.Empty(evidenceCase.EvidenceReferenceIds);
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
