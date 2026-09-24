using SmartCities.Identity;

namespace SmartCities.Api.Identity;

internal static class CanonicalEmailClaimResolver
{
  internal static string? Resolve(
    ExternalAuthenticatedIdentity identity,
    string emailClaimType,
    string? verificationClaimType,
    bool requireVerified)
  {
    ArgumentNullException.ThrowIfNull(identity);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      emailClaimType);

    var emails = identity.Claims
      .Where(
        claim => string.Equals(
          claim.Type,
          emailClaimType,
          StringComparison.Ordinal))
      .Select(static claim => claim.Value.Trim())
      .Where(static value => value.Length > 0)
      .Distinct(StringComparer.OrdinalIgnoreCase)
      .ToArray();

    if (emails.Length == 0)
    {
      return null;
    }

    if (emails.Length != 1)
    {
      throw new AuthenticationCanonicalizationException(
        "The validated external identity contains ambiguous email claims.");
    }

    if (requireVerified)
    {
      if (string.IsNullOrWhiteSpace(
          verificationClaimType))
      {
        throw new AuthenticationCanonicalizationException(
          "Verified canonical email mapping requires a configured verification claim type.");
      }

      var verification = identity.Claims
        .Where(
          claim => string.Equals(
            claim.Type,
            verificationClaimType,
            StringComparison.Ordinal))
        .Select(static claim => claim.Value.Trim())
        .Where(static value => value.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

      if (verification.Length != 1
        || !bool.TryParse(
          verification[0],
          out var verified)
        || !verified)
      {
        return null;
      }
    }

    return emails[0];
  }
}
