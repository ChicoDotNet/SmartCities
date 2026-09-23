using Microsoft.AspNetCore.Mvc;
using SmartCities.Application.Citizens;
using SmartCities.Citizens;
using SmartCities.Evidence;

namespace SmartCities.Api.Citizens;

/// <summary>
/// Exposes the citizen mobility report acceptance use case over HTTP.
/// </summary>
/// <remarks>
/// The controller depends only on the application service. It does not know about repositories,
/// DbContext, EF Core, or relational providers.
/// </remarks>
[ApiController]
[Route("api/citizen/mobility-reports")]
public sealed class CitizenMobilityReportsController : ControllerBase
{
  private readonly ICitizenMobilityReportService service;

  /// <summary>Initializes the controller with the citizen mobility application service.</summary>
  /// <param name="service">Application service responsible for accepting validated reports.</param>
  public CitizenMobilityReportsController(
    ICitizenMobilityReportService service)
  {
    ArgumentNullException.ThrowIfNull(service);
    this.service = service;
  }

  /// <summary>Accepts a citizen mobility report.</summary>
  /// <param name="request">Citizen-facing report contract.</param>
  /// <param name="cancellationToken">Request-abort cancellation token.</param>
  /// <returns>
  /// HTTP 201 when the report creates its authoritative case, or HTTP 200 when the report is an idempotent replay.
  /// </returns>
  [HttpPost]
  [ProducesResponseType<CitizenMobilityReportAcceptanceResponse>(
    StatusCodes.Status201Created)]
  [ProducesResponseType<CitizenMobilityReportAcceptanceResponse>(
    StatusCodes.Status200OK)]
  public async Task<ActionResult<CitizenMobilityReportAcceptanceResponse>> CreateAsync(
    [FromBody] CreateCitizenMobilityReportRequest request,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);

    var evidenceReferences = (request.EvidenceReferences ?? [])
      .Select(static evidence =>
        EvidenceReference.Create(
          evidence.EvidenceId,
          evidence.Kind,
          EvidenceProvenance.Create(
            evidence.SourceSystem,
            evidence.SourceReference,
            evidence.ObservedAt)))
      .ToArray();

    var report = CitizenMobilityReport.Create(
      request.ReportId,
      request.CategoryKey,
      request.LocationReference,
      request.Description,
      evidenceReferences);

    var acceptance = await service
      .AcceptAsync(
        report,
        request.CaseId,
        cancellationToken)
      .ConfigureAwait(false);

    var response = new CitizenMobilityReportAcceptanceResponse(
      acceptance.ReportId,
      acceptance.EvidenceCase.CaseId,
      acceptance.WasCreated);

    if (acceptance.WasCreated)
    {
      return StatusCode(
        StatusCodes.Status201Created,
        response);
    }

    return Ok(response);
  }
}
