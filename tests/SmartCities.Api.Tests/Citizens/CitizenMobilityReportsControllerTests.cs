using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Api.Citizens;
using SmartCities.Application.Citizens;
using SmartCities.Citizens;
using SmartCities.Evidence;
using Xunit;

namespace SmartCities.Api.Tests.Citizens;

public sealed class CitizenMobilityReportsControllerTests
{
  [Fact]
  public void Controller_depends_only_on_the_application_service()
  {
    var constructor = Assert.Single(
      typeof(CitizenMobilityReportsController).GetConstructors());

    var parameter = Assert.Single(constructor.GetParameters());

    Assert.Equal(
      typeof(ICitizenMobilityReportService),
      parameter.ParameterType);
  }

  [Fact]
  public async Task Create_maps_the_http_contract_to_the_domain_and_returns_201_when_created()
  {
    var service = new RecordingCitizenMobilityReportService(wasCreated: true);
    var controller = new CitizenMobilityReportsController(service);
    var observedAt = new DateTimeOffset(
      2026,
      9,
      23,
      7,
      5,
      0,
      TimeSpan.FromHours(-6));

    var result = await controller.CreateAsync(
      new CreateCitizenMobilityReportRequest(
        "report-001",
        "case-001",
        "pedestrian-safety",
        "Intersection AGS-001",
        "Unsafe pedestrian crossing.",
        [
          new CitizenEvidenceReferenceRequest(
            "evidence-photo",
            EvidenceKind.Photograph,
            "citizen-portal",
            "photo:001",
            observedAt),
        ]),
      TestContext.Current.CancellationToken);

    var response = Assert.IsType<ObjectResult>(result.Result);

    Assert.Equal(StatusCodes.Status201Created, response.StatusCode);

    var payload = Assert.IsType<CitizenMobilityReportAcceptanceResponse>(
      response.Value);

    Assert.Equal("report-001", payload.ReportId);
    Assert.Equal("case-001", payload.CaseId);
    Assert.True(payload.WasCreated);

    Assert.NotNull(service.LastReport);
    Assert.Equal("pedestrian-safety", service.LastReport.CategoryKey);
    Assert.Equal("Intersection AGS-001", service.LastReport.LocationReference);
    Assert.Equal("Unsafe pedestrian crossing.", service.LastReport.Description);
    Assert.Equal(["evidence-photo"], service.LastReport.EvidenceReferenceIds);

    var evidence = Assert.Single(service.LastReport.EvidenceReferences);
    Assert.Equal(EvidenceKind.Photograph, evidence.Kind);
    Assert.Equal("citizen-portal", evidence.Provenance.SourceSystem);
    Assert.Equal("photo:001", evidence.Provenance.SourceReference);
    Assert.Equal(observedAt.ToUniversalTime(), evidence.Provenance.ObservedAtUtc);
    Assert.Equal("case-001", service.LastCaseId);
  }

  [Fact]
  public async Task Create_returns_200_with_the_authoritative_case_for_an_idempotent_replay()
  {
    var service = new RecordingCitizenMobilityReportService(wasCreated: false);
    var controller = new CitizenMobilityReportsController(service);

    var result = await controller.CreateAsync(
      new CreateCitizenMobilityReportRequest(
        "report-002",
        "case-candidate",
        "public-transport",
        "Stop AGS-1024",
        "The stop has no accessible boarding path.",
        []),
      TestContext.Current.CancellationToken);

    var response = Assert.IsType<OkObjectResult>(result.Result);
    var payload = Assert.IsType<CitizenMobilityReportAcceptanceResponse>(
      response.Value);

    Assert.Equal("report-002", payload.ReportId);
    Assert.Equal("case-authoritative", payload.CaseId);
    Assert.False(payload.WasCreated);
  }

  [Fact]
  public async Task Get_returns_the_persisted_authoritative_case()
  {
    var service = new RecordingCitizenMobilityReportService(wasCreated: true);
    var controller = new CitizenMobilityReportsController(service);

    await controller.CreateAsync(
      new CreateCitizenMobilityReportRequest(
        "report-get",
        "case-persisted",
        "road-safety",
        "Intersection AGS-200",
        "Unsafe turning movement.",
        []),
      TestContext.Current.CancellationToken);

    var result = await controller.GetAsync(
      "report-get",
      TestContext.Current.CancellationToken);

    var response = Assert.IsType<OkObjectResult>(result.Result);
    var payload = Assert.IsType<CitizenMobilityReportCaseResponse>(
      response.Value);

    Assert.Equal("report-get", payload.ReportId);
    Assert.Equal("case-persisted", payload.CaseId);
  }

  [Fact]
  public async Task Get_returns_404_when_the_report_is_unknown()
  {
    var controller = new CitizenMobilityReportsController(
      new RecordingCitizenMobilityReportService(wasCreated: true));

    var result = await controller.GetAsync(
      "report-missing",
      TestContext.Current.CancellationToken);

    Assert.IsType<NotFoundResult>(result.Result);
  }

  private sealed class RecordingCitizenMobilityReportService
    : ICitizenMobilityReportService
  {
    private readonly Dictionary<string, EvidenceCase> casesByReportId =
      new(StringComparer.Ordinal);
    private readonly bool wasCreated;

    public RecordingCitizenMobilityReportService(bool wasCreated)
    {
      this.wasCreated = wasCreated;
    }

    public CitizenMobilityReport? LastReport { get; private set; }

    public string? LastCaseId { get; private set; }

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

    public Task<CitizenMobilityReportAcceptance> AcceptAsync(
      CitizenMobilityReport report,
      string caseId,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      LastReport = report;
      LastCaseId = caseId;

      var authoritativeCase = report.CreateEvidenceCase(
        wasCreated
          ? caseId
          : "case-authoritative");

      casesByReportId[report.ReportId] = authoritativeCase;

      return Task.FromResult(
        wasCreated
          ? CitizenMobilityReportAcceptance.Created(
              report.ReportId,
              authoritativeCase)
          : CitizenMobilityReportAcceptance.Existing(
              report.ReportId,
              authoritativeCase));
    }
  }
}
