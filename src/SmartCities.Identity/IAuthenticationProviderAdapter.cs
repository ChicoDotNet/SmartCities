namespace SmartCities.Identity;

/// <summary>
/// Normalizes one trusted external authentication provider into the canonical SmartCities identity model.
/// </summary>
/// <remarks>
/// A concrete authentication scheme must validate the credential/token before this adapter is called. The adapter
/// owns provider-specific issuer/tenant recognition plus mapping of provider-native claims into stable SmartCities
/// subject, authority-role, and permission values.
/// </remarks>
public interface IAuthenticationProviderAdapter
{
  /// <summary>Gets the stable SmartCities identifier for this provider adapter.</summary>
  string ProviderId { get; }

  /// <summary>
  /// Determines whether this adapter explicitly accepts the validated provider context.
  /// </summary>
  /// <param name="context">Validated authentication scheme, issuer, and optional tenant context.</param>
  /// <returns><see langword="true"/> only when this adapter owns the context.</returns>
  bool CanHandle(
    AuthenticationProviderContext context);

  /// <summary>
  /// Converts an already authenticated provider-native identity into canonical SmartCities authorization data.
  /// </summary>
  /// <param name="context">Validated provider context accepted by this adapter.</param>
  /// <param name="identity">Validated provider-native subject and claims.</param>
  /// <param name="cancellationToken">Token used to cancel provider-specific entitlement resolution.</param>
  /// <returns>The canonical SmartCities identity.</returns>
  Task<CanonicalIdentity> NormalizeAsync(
    AuthenticationProviderContext context,
    ExternalAuthenticatedIdentity identity,
    CancellationToken cancellationToken = default);
}
