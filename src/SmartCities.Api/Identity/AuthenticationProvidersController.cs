using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace SmartCities.Api.Identity;

/// <summary>
/// Exposes enabled authentication choices and safe provider challenges to the frontend.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/authentication/providers")]
public sealed class AuthenticationProvidersController
  : ControllerBase
{
  private readonly SmartCitiesAuthenticationProviderRegistry registry;

  /// <summary>Initializes authentication-provider discovery over the immutable enabled registry.</summary>
  public AuthenticationProvidersController(
    SmartCitiesAuthenticationProviderRegistry registry)
  {
    ArgumentNullException.ThrowIfNull(registry);
    this.registry = registry;
  }

  /// <summary>Returns exactly the sign-in providers enabled for this deployment.</summary>
  [HttpGet]
  [ProducesResponseType<
    IReadOnlyList<AuthenticationProviderDiscoveryResponse>>(
      StatusCodes.Status200OK)]
  public ActionResult<
    IReadOnlyList<AuthenticationProviderDiscoveryResponse>>
    GetProviders()
  {
    var providers = registry.Providers
      .Select(ToDiscoveryResponse)
      .ToArray();

    return Ok(providers);
  }

  /// <summary>Starts an interactive provider challenge using a safe local return URL.</summary>
  [HttpGet("{providerId}/challenge")]
  public IActionResult ChallengeProvider(
    string providerId,
    [FromQuery] string? returnUrl = null)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      providerId);

    var provider = registry.Providers
      .SingleOrDefault(
        item => string.Equals(
          item.ProviderId,
          providerId,
          StringComparison.Ordinal));

    if (provider is null)
    {
      return NotFound();
    }

    if (provider.ChallengeScheme is null)
    {
      return BadRequest(
        "The selected authentication provider does not use a redirect challenge.");
    }

    var redirectUri = string.IsNullOrWhiteSpace(
      returnUrl)
        ? "/"
        : returnUrl;

    if (!IsSafeLocalReturnUrl(redirectUri))
    {
      return BadRequest(
        "The return URL must be a local application path.");
    }

    return Challenge(
      new AuthenticationProperties
      {
        RedirectUri = redirectUri,
      },
      provider.ChallengeScheme);
  }

  private static AuthenticationProviderDiscoveryResponse ToDiscoveryResponse(
    SmartCitiesAuthenticationProviderDescriptor provider)
  {
    if (provider.Kind
      == SmartCitiesAuthenticationProviderKind.Local)
    {
      return new AuthenticationProviderDiscoveryResponse(
        provider.ProviderId,
        provider.DisplayName,
        "credentials",
        ChallengePath: null,
        SessionPath:
          "/api/authentication/local/session",
        TokenPath:
          "/api/authentication/local/token");
    }

    return new AuthenticationProviderDiscoveryResponse(
      provider.ProviderId,
      provider.DisplayName,
      "redirect",
      $"/api/authentication/providers/{Uri.EscapeDataString(provider.ProviderId)}/challenge",
      SessionPath: null,
      TokenPath: null);
  }

  private static bool IsSafeLocalReturnUrl(
    string returnUrl)
  {
    if (string.IsNullOrWhiteSpace(returnUrl)
      || returnUrl[0] != '/')
    {
      return false;
    }

    if (returnUrl.Length > 1
      && (returnUrl[1] == '/'
        || returnUrl[1] == '\\'))
    {
      return false;
    }

    return !returnUrl.Contains('\\')
      && !returnUrl.Any(char.IsControl);
  }
}
