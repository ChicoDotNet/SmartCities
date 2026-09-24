using SmartCities.Citizens;

namespace SmartCities.Application.Citizens;

/// <summary>
/// Coordinates citizen mobility report acceptance and retrieval without depending on a concrete persistence provider.
/// </summary>
public sealed class CitizenMobilityReportService : ICitizenMobilityReportService
{
  private readonly ICitizenMobilityReportRepository repository;
  private readonly ICitizenMobilityDecisionPipeline decisionPipeline;

  /// <summary>Initializes a report service with persistence and decision-pipeline boundaries.</summary>
  /// <param name="repository">Repository responsible for idempotent case acceptance and retrieval.</param>
  /// <param name="decisionPipeline">Pipeline that ensures the authoritative Evidence Case enters accountable human review.</param>
  public CitizenMobilityReportService(
    ICitizenMobilityReportRepository repository,
    ICitizenMobilityDecisionPipeline decisionPipeline)
  {
    ArgumentNullException.ThrowIfNull(repository);
    ArgumentNullException.ThrowIfNull(decisionPipeline);

    this.repository = repository;
    this.decisionPipeline = decisionPipeline;
  }

  /// <inheritdoc />
  public async Task<CitizenMobilityReportCase?> GetAsync(
    string reportId,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reportId);

    var persisted = await repository
      .GetAsync(reportId, cancellationToken)
      .ConfigureAwait(false);

    if (persisted is not null
      && !string.Equals(
        reportId,
        persisted.ReportId,
        StringComparison.Ordinal))
    {
      throw new InvalidOperationException(
        "Repository result does not match the requested report identifier.");
    }

    return persisted;
  }

  /// <inheritdoc />
  public async Task<CitizenMobilityReportAcceptance> AcceptAsync(
    CitizenMobilityReport report,
    string caseId,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(report);
    ArgumentException.ThrowIfNullOrWhiteSpace(caseId);

    var candidateCase = report.CreateEvidenceCase(caseId);

    var acceptance = await repository
      .GetOrCreateAsync(
        report.ReportId,
        candidateCase,
        cancellationToken)
      .ConfigureAwait(false);

    if (!string.Equals(
      report.ReportId,
      acceptance.ReportId,
      StringComparison.Ordinal))
    {
      throw new InvalidOperationException(
        "Repository acceptance result does not match the submitted report identifier.");
    }

    await decisionPipeline
      .EnsureReviewAsync(
        acceptance.EvidenceCase,
        cancellationToken)
      .ConfigureAwait(false);

    return acceptance;
  }
}
