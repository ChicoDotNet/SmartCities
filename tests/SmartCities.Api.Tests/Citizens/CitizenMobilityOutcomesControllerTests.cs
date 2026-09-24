using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Api.Citizens;
using SmartCities.Api.Localization;
using SmartCities.Application.Citizens;
using SmartCities.Decisions;
using Xunit;

namespace SmartCities.Api.Tests.Citizens;

public sealed class CitizenMobilityOutcomesControllerTests
{
  [Fact]
  public async Task Pending_outcome_is_public_safe_and_localized_in_es_mx()
  {
    var service = new RecordingOutcomeService(
      new CitizenMobilityReportOutcome(
        "report-001",
        "case-001",
        DecisionReviewStatus.PendingHumanReview,
        Disposition: null));
    var controller = CreateController(service, "es-MX");

    var result = await controller.GetAsync(
      "report-001",
      TestContext.Current.CancellationToken);

    var response = Assert.IsType<OkObjectResult>(result.Result);
    var payload = Assert.IsType<CitizenMobilityOutcomeResponse>(
      response.Value);

    Assert.Equal(
      "no-store",
      controller.Response.Headers.CacheControl.ToString());
    Assert.Equal("report-001", payload.ReportId);
    Assert.Equal("case-001", payload.CaseId);
    Assert.Equal("pending-human-review", payload.Status);
    Assert.Null(payload.Disposition);
    Assert.Equal(
      "En revisión humana",
      payload.StatusLabel);
    Assert.Contains(
      "persona autorizada",
      payload.Explanation,
      StringComparison.OrdinalIgnoreCase);

    var json = System.Text.Json.JsonSerializer.Serialize(
      payload);

    Assert.DoesNotContain(
      "recommendation",
      json,
      StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain(
      "authority",
      json,
      StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain(
      "evidence",
      json,
      StringComparison.OrdinalIgnoreCase);
  }

  [Theory]
  [InlineData(DecisionDisposition.Accepted, "accepted", "Aceptada")]
  [InlineData(DecisionDisposition.Modified, "modified", "Modificada")]
  [InlineData(DecisionDisposition.Rejected, "rejected", "Rechazada")]
  public async Task Finalized_outcome_exposes_only_the_human_disposition_and_localized_explanation(
    DecisionDisposition disposition,
    string expectedDisposition,
    string expectedDispositionWord)
  {
    var service = new RecordingOutcomeService(
      new CitizenMobilityReportOutcome(
        "report-final",
        "case-final",
        DecisionReviewStatus.Finalized,
        disposition));
    var controller = CreateController(service, "es-MX");

    var result = await controller.GetAsync(
      "report-final",
      TestContext.Current.CancellationToken);

    var payload = Assert.IsType<CitizenMobilityOutcomeResponse>(
      Assert.IsType<OkObjectResult>(result.Result).Value);

    Assert.Equal("finalized", payload.Status);
    Assert.Equal(expectedDisposition, payload.Disposition);
    Assert.Equal("Revisión concluida", payload.StatusLabel);
    Assert.Contains(
      expectedDispositionWord,
      payload.Explanation,
      StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task Unknown_report_returns_404()
  {
    var controller = CreateController(
      new RecordingOutcomeService(outcome: null),
      "en");

    var result = await controller.GetAsync(
      "missing",
      TestContext.Current.CancellationToken);

    Assert.IsType<NotFoundResult>(result.Result);
  }

  private static CitizenMobilityOutcomesController CreateController(
    ICitizenMobilityOutcomeService service,
    string culture)
  {
    var controller = new CitizenMobilityOutcomesController(
      service,
      new ResxApiLocalizationCatalog());
    var httpContext = new DefaultHttpContext();

    httpContext.Features.Set<
      Microsoft.AspNetCore.Localization.IRequestCultureFeature>(
      new Microsoft.AspNetCore.Localization.RequestCultureFeature(
        new Microsoft.AspNetCore.Localization.RequestCulture(culture),
        provider: null));

    controller.ControllerContext =
      new ControllerContext
      {
        HttpContext = httpContext,
      };

    return controller;
  }

  private sealed class RecordingOutcomeService
    : ICitizenMobilityOutcomeService
  {
    private readonly CitizenMobilityReportOutcome? outcome;

    public RecordingOutcomeService(
      CitizenMobilityReportOutcome? outcome)
    {
      this.outcome = outcome;
    }

    public Task<CitizenMobilityReportOutcome?> GetAsync(
      string reportId,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(outcome);
    }
  }
}
