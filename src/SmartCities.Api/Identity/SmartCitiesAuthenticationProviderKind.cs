namespace SmartCities.Api.Identity;

/// <summary>
/// Identifies the reference authentication-provider composition pattern.
/// </summary>
public enum SmartCitiesAuthenticationProviderKind
{
  /// <summary>Local browser-session cookie plus local JWT bearer.</summary>
  Local = 0,

  /// <summary>Interactive OpenID Connect provider using the generic OIDC adapter.</summary>
  OpenIdConnect = 1,
}
