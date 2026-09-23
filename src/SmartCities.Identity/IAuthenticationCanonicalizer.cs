namespace SmartCities.Identity;

/// <summary>
/// Orchestrates fail-closed provider resolution and canonical identity normalization.
/// </summary>
public interface IAuthenticationCanonicalizer
{
  /// <summary>
  /// Resolves exactly one provider adapter and canonicalizes an already authenticated external identity.
  /// </summary>
  /// <param name="context">Validated provider-routing context.</param>
  /// <param name="identity">Already authenticated provider-native identity.</param>
  /// <param name="cancellationToken">Token used to cancel provider-specific normalization work.</param>
  /// <returns>The provider-neutral SmartCities identity.</returns>
  Task<CanonicalIdentity> CanonicalizeAsync(
    AuthenticationProviderContext context,
    ExternalAuthenticatedIdentity identity,
    CancellationToken cancellationToken = default);
}
