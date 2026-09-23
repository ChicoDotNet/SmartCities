using System.Security.Claims;

namespace SmartCities.Identity;

/// <summary>
/// Converts canonical SmartCities identities into framework claims principals without retaining provider-native claims.
/// </summary>
public static class CanonicalIdentityClaimsExtensions
{
  /// <summary>
  /// Creates an authenticated principal containing only canonical SmartCities identity/authorization claims.
  /// </summary>
  /// <param name="identity">Canonical SmartCities identity.</param>
  /// <param name="authenticationType">Authentication scheme/type that validated the external credential.</param>
  /// <returns>An authenticated principal containing only canonical SmartCities claims.</returns>
  public static ClaimsPrincipal ToClaimsPrincipal(
    this CanonicalIdentity identity,
    string authenticationType)
  {
    ArgumentNullException.ThrowIfNull(identity);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      authenticationType);

    var claims = new List<Claim>
    {
      new(
        SmartCitiesClaimTypes.Subject,
        identity.SubjectId),
      new(
        SmartCitiesClaimTypes.IdentityProvider,
        identity.IdentityProvider),
    };

    claims.AddRange(
      identity.AuthorityRoles.Select(
        static role =>
          new Claim(
            SmartCitiesClaimTypes.AuthorityRole,
            role)));

    claims.AddRange(
      identity.Permissions.Select(
        static permission =>
          new Claim(
            SmartCitiesClaimTypes.Permission,
            permission)));

    var claimsIdentity = new ClaimsIdentity(
      claims,
      authenticationType.Trim(),
      SmartCitiesClaimTypes.Subject,
      SmartCitiesClaimTypes.AuthorityRole);

    return new ClaimsPrincipal(
      claimsIdentity);
  }
}
