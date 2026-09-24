using System.Security.Claims;
using SmartCities.Decisions;
using SmartCities.Identity;

namespace SmartCities.Api.Identity;

/// <summary>
/// Maps an already authenticated and authorized canonical principal into domain human-authority values.
/// </summary>
public static class ClaimsPrincipalHumanAuthorityExtensions
{
  /// <summary>
  /// Creates the existing domain <see cref="HumanAuthority"/> from canonical SmartCities claims.
  /// </summary>
  /// <param name="principal">Authenticated principal whose claims have already crossed the provider adapter boundary.</param>
  /// <returns>The accountable human authority represented by the principal.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="principal"/> is <see langword="null"/>.</exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when the principal is unauthenticated or has missing/ambiguous canonical subject or authority-role claims.
  /// </exception>
  /// <remarks>
  /// This mapping does not itself authorize an action. Callers must first enforce the appropriate SmartCities policy.
  /// </remarks>
  public static HumanAuthority ToHumanAuthority(
    this ClaimsPrincipal principal)
  {
    ArgumentNullException.ThrowIfNull(principal);

    if (principal.Identity?.IsAuthenticated != true)
    {
      throw new InvalidOperationException(
        "An unauthenticated principal cannot become a human authority.");
    }

    var subjectId = GetSingleRequiredClaim(
      principal,
      SmartCitiesClaimTypes.Subject,
      "canonical subject");
    var authorityRole = GetSingleRequiredClaim(
      principal,
      SmartCitiesClaimTypes.AuthorityRole,
      "authority role");

    return HumanAuthority.Create(
      subjectId,
      authorityRole);
  }

  /// <summary>
  /// Creates a domain human authority using one explicit canonical role already granted to the principal.
  /// </summary>
  /// <param name="principal">Authenticated canonical principal.</param>
  /// <param name="authorityRole">Canonical role selected for the accountable action.</param>
  /// <returns>The accountable human authority.</returns>
  public static HumanAuthority ToHumanAuthority(
    this ClaimsPrincipal principal,
    string authorityRole)
  {
    ArgumentNullException.ThrowIfNull(principal);
    ArgumentException.ThrowIfNullOrWhiteSpace(authorityRole);

    if (principal.Identity?.IsAuthenticated != true)
    {
      throw new InvalidOperationException(
        "An unauthenticated principal cannot become a human authority.");
    }

    var subjectId = GetSingleRequiredClaim(
      principal,
      SmartCitiesClaimTypes.Subject,
      "canonical subject");

    var normalizedRole = authorityRole.Trim();
    var hasRole = principal.FindAll(
        SmartCitiesClaimTypes.AuthorityRole)
      .Any(
        claim => string.Equals(
          claim.Value,
          normalizedRole,
          StringComparison.Ordinal));

    if (!hasRole)
    {
      throw new InvalidOperationException(
        "The selected authority role is not granted to the canonical principal.");
    }

    return HumanAuthority.Create(
      subjectId,
      normalizedRole);
  }

  private static string GetSingleRequiredClaim(
    ClaimsPrincipal principal,
    string claimType,
    string description)
  {
    var values = principal.Claims
      .Where(
        claim => string.Equals(
          claim.Type,
          claimType,
          StringComparison.Ordinal))
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
}
