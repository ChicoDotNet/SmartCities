using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Api.FeatureFlags;
using SmartCities.Api.Hosting;
using SmartCities.Api.Localization;
using SmartCities.Application.Citizens;
using SmartCities.Application.FeatureFlags;
using SmartCities.Decisions;

namespace SmartCities.Api.Citizens;

/// <summary>
/// Exposes the public-safe reviewed outcome for an accepted citizen mobility report.
/// </summary>
/// <remarks>
/// This boundary is intentionally privacy-minimal and non-cacheable: localization may change presentation,
/// but the authoritative report, case, review status, and human disposition always come from backend persistence.
/// </remarks>
[ApiController]
[AllowAnonymous]
[RequireFeature(SmartCitiesFeatures.CitizenMobility)]
[Route("api/citizen/mobility-reports/{reportId}/outcome")]
public sealed class CitizenMobilityOutcomesController
  : ControllerBase
{
  private const string PendingStatus =
    "pending-human-review";
  private const string FinalizedStatus =
    "finalized";

  private readonly ICitizenMobilityOutcomeService service;
  private readonly IApiLocalizationCatalog localization;

  /// <summary>Initializes the citizen reviewed-outcome boundary.</summary>
  /// <param name="service">Application query for authoritative report outcome state.</param>
  /// <param name="localization">Public localization catalog.</param>
  public CitizenMobilityOutcomesController(
    ICitizenMobilityOutcomeService service,
    IApiLocalizationCatalog localization)
  {
    ArgumentNullException.ThrowIfNull(service);
    ArgumentNullException.ThrowIfNull(localization);

    this.service = service;
    this.localization = localization;
  }

  /// <summary>
  /// Gets the current citizen-safe human-review outcome for a report.
  /// </summary>
  /// <param name="reportId">Stable citizen report identifier.</param>
  /// <param name="cancellationToken">Request-abort cancellation token.</param>
  /// <returns>HTTP 200 with public-safe state, or HTTP 404 when the report is unknown.</returns>
  [HttpGet]
  [ProducesResponseType<CitizenMobilityOutcomeResponse>(
    StatusCodes.Status200OK)]
  [ProducesResponseType(
    StatusCodes.Status404NotFound)]
  public async Task<ActionResult<CitizenMobilityOutcomeResponse>>
    GetAsync(
      string reportId,
      CancellationToken cancellationToken)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reportId);

    Response.Headers.CacheControl = "no-store";

    var outcome = await service
      .GetAsync(
        reportId,
        cancellationToken)
      .ConfigureAwait(false);

    if (outcome is null)
    {
      return NotFound();
    }

    var culture = ApiRequestCulture.GetUiCulture(
      HttpContext);

    var presentation = GetPresentation(outcome);

    return Ok(
      new CitizenMobilityOutcomeResponse(
        outcome.ReportId,
        outcome.CaseId,
        presentation.Status,
        presentation.Disposition,
        localization.GetString(
          presentation.StatusResourceKey,
          culture),
        localization.GetString(
          presentation.ExplanationResourceKey,
          culture)));
  }

  private static OutcomePresentation GetPresentation(
    CitizenMobilityReportOutcome outcome)
  {
    return outcome.Status switch
    {
      DecisionReviewStatus.PendingHumanReview
        when outcome.Disposition is null =>
          new OutcomePresentation(
            PendingStatus,
            Disposition: null,
            "citizen.mobilityOutcome.status.pendingHumanReview",
            "citizen.mobilityOutcome.explanation.pendingHumanReview"),

      DecisionReviewStatus.Finalized
        when outcome.Disposition is DecisionDisposition disposition =>
          disposition switch
          {
            DecisionDisposition.Accepted =>
              Finalized(
                "accepted",
                "citizen.mobilityOutcome.explanation.accepted"),
            DecisionDisposition.Modified =>
              Finalized(
                "modified",
                "citizen.mobilityOutcome.explanation.modified"),
            DecisionDisposition.Rejected =>
              Finalized(
                "rejected",
                "citizen.mobilityOutcome.explanation.rejected"),
            DecisionDisposition.Deferred =>
              Finalized(
                "deferred",
                "citizen.mobilityOutcome.explanation.deferred"),
            _ => throw new InvalidOperationException(
              "Unsupported final citizen disposition."),
          },

      _ => throw new InvalidOperationException(
        "Citizen outcome review state is internally inconsistent."),
    };
  }

  private static OutcomePresentation Finalized(
    string disposition,
    string explanationResourceKey) =>
    new(
      FinalizedStatus,
      disposition,
      "citizen.mobilityOutcome.status.finalized",
      explanationResourceKey);

  private sealed record OutcomePresentation(
    string Status,
    string? Disposition,
    string StatusResourceKey,
    string ExplanationResourceKey);
}
