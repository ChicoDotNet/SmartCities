using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Application.Administration;
using SmartCities.Identity;

namespace SmartCities.Api.Administration;

/// <summary>
/// Exposes bounded, newest-first immutable Town Hall control-plane audit history.
/// </summary>
[ApiController]
[Route("api/administration/audit")]
public sealed class AdministrationControlPlaneAuditController
  : ControllerBase
{
  private readonly IAdministrationControlPlaneAuditService service;

  /// <summary>Initializes the audit-history HTTP boundary.</summary>
  public AdministrationControlPlaneAuditController(
    IAdministrationControlPlaneAuditService service)
  {
    ArgumentNullException.ThrowIfNull(service);
    this.service = service;
  }

  /// <summary>Gets recent immutable audit entries for the current Town Hall.</summary>
  [HttpGet]
  [Authorize(
    Policy = SmartCitiesPolicies.ReadAdministrationAudit)]
  [ProducesResponseType<
    IReadOnlyList<AdministrationControlPlaneAuditResponse>>(
      StatusCodes.Status200OK)]
  public async Task<
    ActionResult<IReadOnlyList<AdministrationControlPlaneAuditResponse>>>
    GetAsync(
      [FromQuery] int limit = 100,
      CancellationToken cancellationToken = default)
  {
    SetNoStore();

    var entries = await service
      .GetRecentAsync(
        limit,
        cancellationToken)
      .ConfigureAwait(false);

    return Ok(
      entries.Select(
        static entry =>
          new AdministrationControlPlaneAuditResponse(
            entry.EventId,
            entry.OccurredAtUtc,
            entry.ActorSubjectId,
            entry.ActorIdentityProvider,
            entry.Action,
            entry.ResourceType,
            entry.ResourceId,
            entry.Descriptor,
            entry.PreviousValue,
            entry.NewValue,
            entry.CorrelationId))
        .ToArray());
  }

  private void SetNoStore()
  {
    Response.Headers.CacheControl = "no-store";
    Response.Headers.Pragma = "no-cache";
  }
}
