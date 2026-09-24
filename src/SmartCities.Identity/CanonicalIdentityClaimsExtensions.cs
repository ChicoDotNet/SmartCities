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
  public static CanonicalIdentity ToCanonicalIdentity(
    this ClaimsPrincipal principal)
  {
    ArgumentNullException.ThrowIfNull(principal);

    if (principal.Identity?.IsAuthenticated != true)
    {
      throw new InvalidOperationException(
        "An unauthenticated principal cannot represent a canonical SmartCities identity.");
    }

    var subject = GetSingleRequiredClaim(
      principal,
      SmartCitiesClaimTypes.Subject,
      "canonical subject");
    var provider = GetSingleRequiredClaim(
      principal,
      SmartCitiesClaimTypes.IdentityProvider,
      "canonical identity provider");

    var roles = GetDistinctClaims(
      principal,
      SmartCitiesClaimTypes.AuthorityRole);
    var permissions = GetDistinctClaims(
      principal,
      SmartCitiesClaimTypes.Permission);

    return CanonicalIdentity.Create(
      provider,
      subject,
      roles,
      permissions);
  }

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

  private static string GetSingleRequiredClaim(
    ClaimsPrincipal principal,
    string claimType,
    string description)
  {
    var values = principal.FindAll(claimType)
      .Select(static claim => claim.Value)
      .Where(
        static value =>
          !string.IsNullOrWhiteSpace(value))
      .Distinct(StringComparer.Ordinal)
      .ToArray();

    if (values.Length != 1)
    {
      throw new InvalidOperationException(
        $"The principal must contain exactly one {description} claim.");
    }

    return values[0];
  }

  private static string[] GetDistinctClaims(
    ClaimsPrincipal principal,
    string claimType) =>
    principal.FindAll(claimType)
      .Select(static claim => claim.Value)
      .Where(
        static value =>
          !string.IsNullOrWhiteSpace(value))
      .Distinct(StringComparer.Ordinal)
      .ToArray();
}
