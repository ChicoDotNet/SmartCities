using SmartCities.Application.Citizens;
using SmartCities.Citizens;
using SmartCities.Evidence;
using Xunit;

namespace SmartCities.Application.Tests.Citizens;

public sealed class CitizenMobilityReportServiceTests
{
  [Fact]
  public async Task Accepting_a_valid_report_creates_exactly_one_case()
  {
    var repository = new RecordingCitizenMobilityReportRepository();
    var service = new CitizenMobilityReportService(repository);
    var report = CreateReport("report-001");

    var result = await service.AcceptAsync(
      report,
      "case-001",
      TestContext.Current.CancellationToken);

    Assert.True(result.WasCreated);
    Assert.Equal("report-001", result.ReportId);
    Assert.Equal("case-001", result.EvidenceCase.CaseId);
    Assert.Equal(1, repository.CreatedCaseCount);
  }

  [Fact]
  public async Task Repeating_the_same_report_returns_the_original_case_without_creating_another()
  {
    var repository = new RecordingCitizenMobilityReportRepository();
    var service = new CitizenMobilityReportService(repository);
    var report = CreateReport("report-002");

    var first = await service.AcceptAsync(
      report,
      "case-original",
      TestContext.Current.CancellationToken);
    var second = await service.AcceptAsync(
      report,
      "case-ignored",
      TestContext.Current.CancellationToken);

    Assert.True(first.WasCreated);
    Assert.False(second.WasCreated);
    Assert.Equal("case-original", second.EvidenceCase.CaseId);
    Assert.Equal(1, repository.CreatedCaseCount);
  }

  [Fact]
  public async Task Getting_an_existing_report_returns_its_authoritative_case()
  {
    var repository = new RecordingCitizenMobilityReportRepository();
    var service = new CitizenMobilityReportService(repository);
    var report = CreateReport("report-recover");

    await service.AcceptAsync(
      report,
      "case-persisted",
      TestContext.Current.CancellationToken);

    var recovered = await service.GetAsync(
      report.ReportId,
      TestContext.Current.CancellationToken);

    Assert.NotNull(recovered);
    Assert.Equal("report-recover", recovered.ReportId);
    Assert.Equal("case-persisted", recovered.EvidenceCase.CaseId);
  }

  [Fact]
  public async Task Getting_an_unknown_report_returns_null()
  {
    var service = new CitizenMobilityReportService(
      new RecordingCitizenMobilityReportRepository());

    var recovered = await service.GetAsync(
      "report-missing",
      TestContext.Current.CancellationToken);

    Assert.Null(recovered);
  }

  [Fact]
  public async Task Service_passes_the_domain_case_to_the_repository_contract()
  {
    var repository = new RecordingCitizenMobilityReportRepository();
    var service = new CitizenMobilityReportService(repository);
    var report = CreateReport("report-003");

    await service.AcceptAsync(
      report,
      "case-003",
      TestContext.Current.CancellationToken);

    Assert.Equal("report-003", repository.LastReportId);
    Assert.NotNull(repository.LastCandidateCase);
    Assert.Equal("case-003", repository.LastCandidateCase.CaseId);
    Assert.Equal(report.Description, repository.LastCandidateCase.Subject);
    Assert.Equal(report.EvidenceReferenceIds, repository.LastCandidateCase.EvidenceReferenceIds);
  }

  private static CitizenMobilityReport CreateReport(string reportId) =>
    CitizenMobilityReport.Create(
      reportId,
      "pedestrian-safety",
      "Intersection AGS-001",
      "Unsafe pedestrian crossing.",
      [
        EvidenceReference.Create(
          "evidence-001",
          EvidenceKind.CitizenStatement,
          EvidenceProvenance.Create(
            "citizen-portal",
            $"submission:{reportId}",
            new DateTimeOffset(2026, 9, 23, 18, 0, 0, TimeSpan.Zero))),
      ]);

  private sealed class RecordingCitizenMobilityReportRepository
    : ICitizenMobilityReportRepository
  {
    private readonly Dictionary<string, EvidenceCase> casesByReportId =
      new(StringComparer.Ordinal);

    public int CreatedCaseCount { get; private set; }

    public string? LastReportId { get; private set; }

    public EvidenceCase? LastCandidateCase { get; private set; }

    public Task<CitizenMobilityReportCase?> GetAsync(
      string reportId,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      return Task.FromResult(
        casesByReportId.TryGetValue(reportId, out var evidenceCase)
          ? CitizenMobilityReportCase.Create(reportId, evidenceCase)
          : null);
    }

    public Task<CitizenMobilityReportAcceptance> GetOrCreateAsync(
      string reportId,
      EvidenceCase candidateCase,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      LastReportId = reportId;
      LastCandidateCase = candidateCase;

      if (casesByReportId.TryGetValue(reportId, out var existing))
      {
        return Task.FromResult(
          CitizenMobilityReportAcceptance.Existing(reportId, existing));
      }

      casesByReportId.Add(reportId, candidateCase);
      CreatedCaseCount++;

      return Task.FromResult(
        CitizenMobilityReportAcceptance.Created(reportId, candidateCase));
    }
  }
}
