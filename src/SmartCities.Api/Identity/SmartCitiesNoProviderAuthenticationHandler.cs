using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SmartCities.Api.Identity;

/// <summary>
/// Provides a safe challenge when protected endpoints exist but no concrete authentication provider is enabled.
/// </summary>
public sealed class SmartCitiesNoProviderAuthenticationHandler
  : AuthenticationHandler<AuthenticationSchemeOptions>
{
  /// <summary>Gets the provider-neutral fallback challenge scheme name.</summary>
  public const string SchemeName =
    "SmartCities.NoAuthenticationProvider";

  /// <summary>Initializes the fallback authentication handler.</summary>
  public SmartCitiesNoProviderAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : base(options, logger, encoder)
  {
  }

  /// <inheritdoc />
  protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
    Task.FromResult(
      AuthenticateResult.NoResult());

  /// <inheritdoc />
  protected override Task HandleChallengeAsync(
    AuthenticationProperties properties)
  {
    Response.StatusCode =
      StatusCodes.Status401Unauthorized;

    return Task.CompletedTask;
  }
}
