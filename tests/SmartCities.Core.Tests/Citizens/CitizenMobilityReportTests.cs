using SmartCities.Citizens;
using SmartCities.Evidence;
using Xunit;

namespace SmartCities.Core.Tests.Citizens;

public sealed class CitizenMobilityReportTests
{
  [Fact]
  public void Valid_report_creates_one_evidence_case_with_the_same_evidence()
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

    var evidenceCase = report.CreateEvidenceCase("case-001");

    Assert.Equal("case-001", evidenceCase.CaseId);
    Assert.Equal(report.Description, evidenceCase.Subject);
    Assert.Equal(
      ["evidence-photo", "evidence-statement"],
      evidenceCase.EvidenceReferenceIds);
  }

  [Fact]
  public void Report_preserves_stable_category_and_location_reference()
  {
    var report = CitizenMobilityReport.Create(
      "report-002",
      "public-transport",
      "Stop AGS-1024",
      "The stop has no accessible boarding path.",
      []);

    Assert.Equal("report-002", report.ReportId);
    Assert.Equal("public-transport", report.CategoryKey);
    Assert.Equal("Stop AGS-1024", report.LocationReference);
    Assert.Equal("The stop has no accessible boarding path.", report.Description);
  }

  [Theory]
  [InlineData("", "pedestrian-safety", "location", "description")]
  [InlineData("report-003", "", "location", "description")]
  [InlineData("report-003", "pedestrian-safety", "", "description")]
  [InlineData("report-003", "pedestrian-safety", "location", "")]
  public void Report_rejects_missing_required_input(
    string reportId,
    string categoryKey,
    string locationReference,
    string description)
  {
    Assert.Throws<ArgumentException>(
      () => CitizenMobilityReport.Create(
        reportId,
        categoryKey,
        locationReference,
        description,
        []));
  }

  [Fact]
  public void Report_allows_no_optional_evidence()
  {
    var report = CitizenMobilityReport.Create(
      "report-004",
      "traffic-operation",
      "Intersection AGS-001",
      "Signal timing causes long queues.",
      []);

    var evidenceCase = report.CreateEvidenceCase("case-004");

    Assert.Empty(report.EvidenceReferences);
    Assert.Empty(evidenceCase.EvidenceReferences);
  }

  [Fact]
  public void Report_defensively_copies_evidence()
  {
    var evidence = new List<EvidenceReference>
    {
      CreateEvidence("evidence-001", EvidenceKind.CitizenStatement),
    };

    var report = CitizenMobilityReport.Create(
      "report-005",
      "other",
      "Citizen supplied location",
      "Observed mobility problem.",
      evidence);

    evidence.Add(CreateEvidence("evidence-002", EvidenceKind.Document));

    Assert.Single(report.EvidenceReferences);
    Assert.Equal(["evidence-001"], report.EvidenceReferenceIds);
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
        new DateTimeOffset(2026, 9, 23, 17, 0, 0, TimeSpan.Zero)));
}
