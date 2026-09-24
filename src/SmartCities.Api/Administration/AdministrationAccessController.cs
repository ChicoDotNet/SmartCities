using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Application.Administration;
using SmartCities.Identity;

namespace SmartCities.Api.Administration;

/// <summary>
/// Exposes the backend-authoritative Administration admission state.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/administration/access")]
public sealed class AdministrationAccessController
  : ControllerBase
{
  private readonly IAdministrationAccessService service;
  private readonly AdministrationBootstrapConfiguration bootstrap;

  /// <summary>Initializes the Administration access boundary.</summary>
  public AdministrationAccessController(
    IAdministrationAccessService service,
    AdministrationBootstrapConfiguration bootstrap)
  {
    ArgumentNullException.ThrowIfNull(service);
    ArgumentNullException.ThrowIfNull(bootstrap);
    this.service = service;
    this.bootstrap = bootstrap;
  }

  /// <summary>Gets current Town Hall Administration admission state.</summary>
  [HttpGet]
  [ProducesResponseType<AdministrationAccessResponse>(
    StatusCodes.Status200OK)]
  public async Task<ActionResult<AdministrationAccessResponse>>
    GetAccessAsync(
      CancellationToken cancellationToken)
  {
    SetNoStore();

    var bootstrapAvailable =
      bootstrap.IsEnabled
      && await service
        .IsBootstrapAvailableAsync(cancellationToken)
        .ConfigureAwait(false);

    if (User.Identity?.IsAuthenticated != true)
    {
      return Ok(
        new AdministrationAccessResponse(
          service.TownHallId,
          Authorized: false,
          bootstrapAvailable));
    }

    CanonicalIdentity canonical;

    try
    {
      canonical = User.ToCanonicalIdentity();
    }
    catch (InvalidOperationException)
    {
      return Ok(
        new AdministrationAccessResponse(
          service.TownHallId,
          Authorized: false,
          bootstrapAvailable));
    }

    var authorized = await service
      .IsAuthorizedAsync(
        AdministrationIdentity.Create(
          canonical.SubjectId,
          canonical.EmailAddress),
        cancellationToken)
      .ConfigureAwait(false);

    return Ok(
      new AdministrationAccessResponse(
        service.TownHallId,
        authorized,
        bootstrapAvailable));
  }

  private void SetNoStore()
  {
    Response.Headers.CacheControl = "no-store";
    Response.Headers.Pragma = "no-cache";
  }
}
