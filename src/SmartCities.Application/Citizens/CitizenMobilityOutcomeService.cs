using SmartCities.Application.HumanOversight;

namespace SmartCities.Application.Citizens;

/// <summary>
/// Resolves the public-safe reviewed outcome from authoritative report and human-review persistence.
/// </summary>
public sealed class CitizenMobilityOutcomeService
  : ICitizenMobilityOutcomeService
{
  private readonly ICitizenMobilityReportRepository reportRepository;
  private readonly IDecisionReviewRepository reviewRepository;

  /// <summary>Initializes the citizen outcome query.</summary>
  /// <param name="reportRepository">Authoritative report repository.</param>
  /// <param name="reviewRepository">Authoritative human-review repository.</param>
  public CitizenMobilityOutcomeService(
    ICitizenMobilityReportRepository reportRepository,
    IDecisionReviewRepository reviewRepository)
  {
    ArgumentNullException.ThrowIfNull(reportRepository);
    ArgumentNullException.ThrowIfNull(reviewRepository);

    this.reportRepository = reportRepository;
    this.reviewRepository = reviewRepository;
  }

  /// <inheritdoc />
  public async Task<CitizenMobilityReportOutcome?> GetAsync(
    string reportId,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reportId);

    var report = await reportRepository
      .GetAsync(
        reportId,
        cancellationToken)
      .ConfigureAwait(false);

    if (report is null)
    {
      return null;
    }

    var review = await reviewRepository
      .GetByEvidenceCaseIdAsync(
        report.EvidenceCase.CaseId,
        cancellationToken)
      .ConfigureAwait(false);

    if (review is null)
    {
      throw new InvalidOperationException(
        "An accepted citizen report is missing its authoritative human-review record.");
    }

    return new CitizenMobilityReportOutcome(
      report.ReportId,
      report.EvidenceCase.CaseId,
      review.Status,
      review.Disposition);
  }
}
