using SmartCities.Identity;

namespace SmartCities.Api.Identity;

/// <summary>
/// Provides the trusted ASP.NET Core feature handoff used by concrete authentication integrations.
/// </summary>
public static class ValidatedExternalAuthenticationHttpContextExtensions
{
  /// <summary>
  /// Stores validated external provider context/identity for subsequent SmartCities canonicalization.
  /// </summary>
  /// <param name="context">Current HTTP request context.</param>
  /// <param name="providerContext">Validated provider-routing context.</param>
  /// <param name="externalIdentity">Validated provider-native identity.</param>
  /// <remarks>
  /// Call this only after the concrete authentication mechanism has successfully validated the credential/session.
  /// </remarks>
  public static void SetValidatedExternalAuthentication(
    this HttpContext context,
    AuthenticationProviderContext providerContext,
    ExternalAuthenticatedIdentity externalIdentity)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(providerContext);
    ArgumentNullException.ThrowIfNull(externalIdentity);

    context.Features.Set<IValidatedExternalAuthenticationFeature>(
      new ValidatedExternalAuthenticationFeature(
        providerContext,
        externalIdentity));
  }
}
