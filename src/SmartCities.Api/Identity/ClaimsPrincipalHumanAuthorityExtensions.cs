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
