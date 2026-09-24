using SmartCities.Application.Citizens;
using SmartCities.Application.HumanOversight;
using SmartCities.Criterion;
using SmartCities.Decisions;
using SmartCities.Evidence;
using Xunit;

namespace SmartCities.Application.Tests.Citizens;

public sealed class CitizenMobilityOutcomeServiceTests
{
  [Fact]
  public async Task Known_report_returns_pending_human_review_without_a_disposition()
  {
    var evidenceCase = CreateCase("case-pending");
    var service = new CitizenMobilityOutcomeService(
      new RecordingReportRepository(
        CitizenMobilityReportCase.Create(
          "report-pending",
          evidenceCase)),
      new RecordingReviewRepository(
        PendingReview(
          evidenceCase.CaseId)));

    var outcome = await service.GetAsync(
      "report-pending",
      TestContext.Current.CancellationToken);

    Assert.NotNull(outcome);
    Assert.Equal("report-pending", outcome.ReportId);
    Assert.Equal("case-pending", outcome.CaseId);
    Assert.Equal(
      DecisionReviewStatus.PendingHumanReview,
      outcome.Status);
    Assert.Null(outcome.Disposition);
  }

  [Fact]
  public async Task Known_report_returns_the_authoritative_final_human_disposition()
  {
    var evidenceCase = CreateCase("case-final");
    var authority = HumanAuthority.Create(
      "reviewer-001",
      "mobility-reviewer");
    var finalized = PendingReview(
        evidenceCase.CaseId)
      .Finalize(
        authority,
        DecisionDisposition.Modified);

    var service = new CitizenMobilityOutcomeService(
      new RecordingReportRepository(
        CitizenMobilityReportCase.Create(
          "report-final",
          evidenceCase)),
      new RecordingReviewRepository(finalized));

    var outcome = await service.GetAsync(
      "report-final",
      TestContext.Current.CancellationToken);

    Assert.NotNull(outcome);
    Assert.Equal(
      DecisionReviewStatus.Finalized,
      outcome.Status);
    Assert.Equal(
      DecisionDisposition.Modified,
      outcome.Disposition);
  }

  [Fact]
  public async Task Unknown_report_returns_null_without_querying_review_state()
  {
    var reviews = new RecordingReviewRepository(
      review: null);
    var service = new CitizenMobilityOutcomeService(
      new RecordingReportRepository(
        report: null),
      reviews);

    var outcome = await service.GetAsync(
      "missing",
      TestContext.Current.CancellationToken);

    Assert.Null(outcome);
    Assert.Equal(0, reviews.CaseLookupCount);
  }

  [Fact]
  public async Task Accepted_report_without_its_review_fails_closed()
  {
    var evidenceCase = CreateCase("case-missing-review");
    var service = new CitizenMobilityOutcomeService(
      new RecordingReportRepository(
        CitizenMobilityReportCase.Create(
          "report-missing-review",
          evidenceCase)),
      new RecordingReviewRepository(
        review: null));

    await Assert.ThrowsAsync<InvalidOperationException>(
      () => service.GetAsync(
        "report-missing-review",
        TestContext.Current.CancellationToken));
  }

  private static EvidenceCase CreateCase(
    string caseId) =>
    EvidenceCase.Create(
      caseId,
      "Citizen-safe subject.",
      []);

  private static DecisionReview PendingReview(
    string caseId) =>
    DecisionReview.Pending(
      CriterionDecisionTrace.Create(
        requestId: $"request:{caseId}",
        recommendationId: $"recommendation:{caseId}",
        recommendation:
          CriterionRecommendation.RequiresHumanReview,
        requiresHumanReview: true,
        evidenceReferenceIds: [],
        publicExplanation: "Public-safe explanation.",
        evidenceCaseId: caseId));

  private sealed class RecordingReportRepository
    : ICitizenMobilityReportRepository
  {
    private readonly CitizenMobilityReportCase? report;

    public RecordingReportRepository(
      CitizenMobilityReportCase? report)
    {
      this.report = report;
    }

    public Task<CitizenMobilityReportCase?> GetAsync(
      string reportId,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      return Task.FromResult(
        report is not null
        && string.Equals(
          report.ReportId,
          reportId,
          StringComparison.Ordinal)
          ? report
          : null);
    }

    public Task<CitizenMobilityReportAcceptance> GetOrCreateAsync(
      string reportId,
      EvidenceCase candidateCase,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();
  }

  private sealed class RecordingReviewRepository
    : IDecisionReviewRepository
  {
    private readonly DecisionReview? review;

    public RecordingReviewRepository(
      DecisionReview? review)
    {
      this.review = review;
    }

    public int CaseLookupCount { get; private set; }

    public Task AddPendingAsync(
      DecisionReview review,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<DecisionReview> GetOrAddPendingAsync(
      DecisionReview review,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<DecisionReview?> GetByEvidenceCaseIdAsync(
      string evidenceCaseId,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      CaseLookupCount++;

      return Task.FromResult(
        review is not null
        && string.Equals(
          review.EvidenceCaseId,
          evidenceCaseId,
          StringComparison.Ordinal)
          ? review
          : null);
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
