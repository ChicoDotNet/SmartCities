using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Identity;

namespace SmartCities.Api.Identity;

/// <summary>
/// Completes the local credential flow into either an encrypted browser session or a JWT bearer token.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/authentication/local")]
public sealed class LocalAuthenticationController
  : ControllerBase
{
  private readonly ILocalAuthenticationService authentication;

  /// <summary>Initializes the local authentication HTTP boundary.</summary>
  public LocalAuthenticationController(
    ILocalAuthenticationService authentication)
  {
    ArgumentNullException.ThrowIfNull(authentication);
    this.authentication = authentication;
  }

  /// <summary>Validates local credentials and establishes the canonical browser-session cookie.</summary>
  [HttpPost("session")]
  [ProducesResponseType<LocalSessionResponse>(
    StatusCodes.Status200OK)]
  [ProducesResponseType(
    StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(
    StatusCodes.Status404NotFound)]
  public async Task<ActionResult<LocalSessionResponse>> CreateSessionAsync(
    [FromBody] LocalCredentialRequest request,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);

    if (!authentication.IsEnabled)
    {
      return NotFound();
    }

    var identity = await authentication
      .AuthenticateAsync(
        request.UserName,
        request.Password,
        cancellationToken)
      .ConfigureAwait(false);

    if (identity is null)
    {
      return Unauthorized();
    }

    Response.Headers.CacheControl =
      "no-store";
    Response.Headers.Pragma =
      "no-cache";

    await HttpContext.SignInAsync(
        SmartCitiesAuthenticationSchemes.Session,
        identity.ToClaimsPrincipal(
          SmartCitiesAuthenticationSchemes.Session),
        new AuthenticationProperties
        {
          IsPersistent = false,
          AllowRefresh = true,
        })
      .ConfigureAwait(false);

    return Ok(
      new LocalSessionResponse(
        identity.SubjectId,
        identity.IdentityProvider,
        identity.AuthorityRoles));
  }

  /// <summary>Validates local credentials and issues a short-lived local JWT bearer token.</summary>
  [HttpPost("token")]
  [ProducesResponseType<LocalAccessTokenResponse>(
    StatusCodes.Status200OK)]
  [ProducesResponseType(
    StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(
    StatusCodes.Status404NotFound)]
  public async Task<ActionResult<LocalAccessTokenResponse>> CreateTokenAsync(
    [FromBody] LocalCredentialRequest request,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);

    if (!authentication.IsEnabled)
    {
      return NotFound();
    }

    var token = await authentication
      .IssueAccessTokenAsync(
        request.UserName,
        request.Password,
        cancellationToken)
      .ConfigureAwait(false);

    if (token is null)
    {
      return Unauthorized();
    }

    Response.Headers.CacheControl =
      "no-store";
    Response.Headers.Pragma =
      "no-cache";

    return Ok(
      new LocalAccessTokenResponse(
        token.Value,
        "Bearer",
        token.ExpiresInSeconds));
  }
}
