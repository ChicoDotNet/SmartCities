using SmartCities.Citizens;

namespace SmartCities.Application.Citizens;

/// <summary>
/// Coordinates citizen mobility report acceptance without depending on a concrete persistence provider.
/// </summary>
public sealed class CitizenMobilityReportService : ICitizenMobilityReportService
{
  private readonly ICitizenMobilityReportRepository repository;

  /// <summary>Initializes a report service with its repository boundary.</summary>
  /// <param name="repository">Repository responsible for idempotent case acceptance.</param>
  public CitizenMobilityReportService(
    ICitizenMobilityReportRepository repository)
  {
    ArgumentNullException.ThrowIfNull(repository);
    this.repository = repository;
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

    return acceptance;
  }
}
