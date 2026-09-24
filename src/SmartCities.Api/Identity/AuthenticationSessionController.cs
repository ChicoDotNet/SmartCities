using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Identity;

namespace SmartCities.Api.Identity;

/// <summary>
/// Exposes the authoritative canonical browser-session state and local sign-out operation.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/authentication/session")]
public sealed class AuthenticationSessionController
  : ControllerBase
{
  private const string BrowserRequestHeaderName =
    "X-SmartCities-Request";
  private const string BrowserRequestHeaderValue =
    "browser";

  private readonly IAuthenticationSchemeProvider schemes;

  /// <summary>Initializes the canonical session HTTP boundary.</summary>
  public AuthenticationSessionController(
    IAuthenticationSchemeProvider schemes)
  {
    ArgumentNullException.ThrowIfNull(schemes);
    this.schemes = schemes;
  }

  /// <summary>
  /// Returns the backend-authoritative canonical authentication state for the current request.
  /// </summary>
  [HttpGet]
  [ProducesResponseType<AuthenticationSessionResponse>(
    StatusCodes.Status200OK)]
  public ActionResult<AuthenticationSessionResponse>
    GetCurrentSession()
  {
    SetNoStore();

    if (User.Identity?.IsAuthenticated != true)
    {
      return Ok(
        new AuthenticationSessionResponse(
          Authenticated: false,
          SubjectId: null,
          IdentityProvider: null,
          AuthorityRoles: Array.Empty<string>(),
          Permissions: Array.Empty<string>()));
    }

    var identity = User.ToCanonicalIdentity();

    return Ok(
      new AuthenticationSessionResponse(
        Authenticated: true,
        identity.SubjectId,
        identity.IdentityProvider,
        identity.AuthorityRoles,
        identity.Permissions));
  }

  /// <summary>
  /// Expires the shared SmartCities browser-session cookie without initiating provider-specific federated logout.
  /// </summary>
  [HttpPost("logout")]
  [ProducesResponseType(
    StatusCodes.Status204NoContent)]
  [ProducesResponseType(
    StatusCodes.Status403Forbidden)]
  public async Task<IActionResult> LogoutAsync()
  {
    SetNoStore();

    if (!string.Equals(
        Request.Headers[
          BrowserRequestHeaderName]
          .ToString(),
        BrowserRequestHeaderValue,
        StringComparison.Ordinal))
    {
      return StatusCode(
        StatusCodes.Status403Forbidden);
    }

    var sessionScheme = await schemes
      .GetSchemeAsync(
        SmartCitiesAuthenticationSchemes.Session)
      .ConfigureAwait(false);

    if (sessionScheme is not null)
    {
      await HttpContext
        .SignOutAsync(
          SmartCitiesAuthenticationSchemes.Session)
        .ConfigureAwait(false);
    }

    return NoContent();
  }

  private void SetNoStore()
  {
    Response.Headers.CacheControl =
      "no-store";
    Response.Headers.Pragma =
      "no-cache";
  }
}
