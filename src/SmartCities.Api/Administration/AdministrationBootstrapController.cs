using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartCities.Api.Identity;
using SmartCities.Application.Administration;
using SmartCities.Application.FeatureFlags;
using SmartCities.Identity;

namespace SmartCities.Api.Administration;

/// <summary>
/// Establishes the temporary empty-whitelist Administration bootstrap session.
/// </summary>
/// <remarks>
/// The endpoint is rate-limited and becomes unavailable as soon as any whitelist rule exists.
/// Existing bootstrap cookies are re-evaluated by Administration authorization and lose admission immediately.
/// </remarks>
[ApiController]
[AllowAnonymous]
[EnableRateLimiting("administration-bootstrap")]
[Route("api/administration/bootstrap")]
public sealed class AdministrationBootstrapController
  : ControllerBase
{
  private readonly IAdministrationAccessService administration;
  private readonly AdministrationBootstrapConfiguration configuration;
  private readonly IAuthenticationSchemeProvider schemes;

  /// <summary>Initializes the bootstrap session boundary.</summary>
  public AdministrationBootstrapController(
    IAdministrationAccessService administration,
    AdministrationBootstrapConfiguration configuration,
    IAuthenticationSchemeProvider schemes)
  {
    ArgumentNullException.ThrowIfNull(administration);
    ArgumentNullException.ThrowIfNull(configuration);
    ArgumentNullException.ThrowIfNull(schemes);

    this.administration = administration;
    this.configuration = configuration;
    this.schemes = schemes;
  }

  /// <summary>
  /// Validates the deployment bootstrap credential and creates the shared canonical browser session.
  /// </summary>
  [HttpPost("session")]
  [ProducesResponseType<AdministrationBootstrapSessionResponse>(
    StatusCodes.Status200OK)]
  [ProducesResponseType(
    StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(
    StatusCodes.Status404NotFound)]
  public async Task<ActionResult<AdministrationBootstrapSessionResponse>>
    CreateSessionAsync(
      [FromBody] LocalCredentialRequest request,
      CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);
    SetNoStore();

    if (!configuration.IsEnabled
      || !await administration
        .IsBootstrapAvailableAsync(cancellationToken)
        .ConfigureAwait(false)
      || await schemes
        .GetSchemeAsync(
          SmartCitiesAuthenticationSchemes.Session)
        .ConfigureAwait(false) is null)
    {
      return NotFound();
    }

    if (string.IsNullOrWhiteSpace(request.UserName)
      || string.IsNullOrEmpty(request.Password)
      || !string.Equals(
        request.UserName.Trim(),
        configuration.UserName,
        StringComparison.OrdinalIgnoreCase)
      || !FixedTimePasswordEquals(
        request.Password,
        configuration.Password!))
    {
      return Unauthorized();
    }

    var canonical = CanonicalIdentity.Create(
      identityProvider: "local-bootstrap",
      subjectId:
        AdministrationAccessService.BootstrapCanonicalSubject,
      authorityRoles:
      [
        "town-hall-admin",
      ],
      permissions:
      [
        SmartCitiesPermissions.ConfigureFeatureFlags,
        SmartCitiesPermissions.ManageAdministrationWhitelist,
        SmartCitiesPermissions.ManageAdministrationGrants,
        .. SmartCitiesFeatures.All.Select(
          SmartCitiesFeaturePermissions.Configure),
      ],
      emailAddress:
        AdministrationAccessService.BootstrapEmailAddress);

    await HttpContext
      .SignInAsync(
        SmartCitiesAuthenticationSchemes.Session,
        canonical.ToClaimsPrincipal(
          SmartCitiesAuthenticationSchemes.Session),
        new AuthenticationProperties
        {
          IsPersistent = false,
          AllowRefresh = true,
        })
      .ConfigureAwait(false);

    return Ok(
      new AdministrationBootstrapSessionResponse(
        canonical.SubjectId,
        configuration.UserName));
  }

  private static bool FixedTimePasswordEquals(
    string supplied,
    string expected)
  {
    var suppliedHash = SHA256.HashData(
      Encoding.UTF8.GetBytes(supplied));
    var expectedHash = SHA256.HashData(
      Encoding.UTF8.GetBytes(expected));

    return CryptographicOperations.FixedTimeEquals(
      suppliedHash,
      expectedHash);
  }

  private void SetNoStore()
  {
    Response.Headers.CacheControl = "no-store";
    Response.Headers.Pragma = "no-cache";
  }
}
