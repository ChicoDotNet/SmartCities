namespace SmartCities.Identity;

/// <summary>
/// Resolves one provider adapter and enforces canonical provider-identity invariants.
/// </summary>
public sealed class AuthenticationCanonicalizer
  : IAuthenticationCanonicalizer
{
  private readonly IAuthenticationProviderAdapterResolver resolver;

  /// <summary>Initializes the canonicalizer with the configured provider-adapter resolver.</summary>
  /// <param name="resolver">Fail-closed authentication-provider adapter resolver.</param>
  public AuthenticationCanonicalizer(
    IAuthenticationProviderAdapterResolver resolver)
  {
    ArgumentNullException.ThrowIfNull(resolver);
    this.resolver = resolver;
  }

  /// <inheritdoc />
  public async Task<CanonicalIdentity> CanonicalizeAsync(
    AuthenticationProviderContext context,
    ExternalAuthenticatedIdentity identity,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(identity);

    IAuthenticationProviderAdapter adapter;

    try
    {
      adapter = resolver.Resolve(context);
    }
    catch (InvalidOperationException exception)
    {
      throw new AuthenticationCanonicalizationException(
        "The validated external identity could not be routed to exactly one trusted provider adapter.",
        exception);
    }

    var canonical = await adapter
      .NormalizeAsync(
        context,
        identity,
        cancellationToken)
      .ConfigureAwait(false);

    if (!string.Equals(
      adapter.ProviderId,
      canonical.IdentityProvider,
      StringComparison.Ordinal))
    {
      throw new AuthenticationCanonicalizationException(
        "The authentication provider adapter returned a canonical identity for a different provider.");
    }

    return canonical;
  }
}
