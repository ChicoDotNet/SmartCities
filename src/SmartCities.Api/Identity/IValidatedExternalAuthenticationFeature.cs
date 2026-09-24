using SmartCities.Identity;

namespace SmartCities.Api.Identity;

/// <summary>
/// Carries the trusted result of concrete authentication into the SmartCities canonicalization middleware.
/// </summary>
/// <remarks>
/// Concrete authentication integrations populate this feature only after credential/protocol validation succeeds.
/// Provider-native claims remain confined to this boundary until canonicalization replaces the request principal.
/// </remarks>
public interface IValidatedExternalAuthenticationFeature
{
  /// <summary>Gets the validated provider-routing context.</summary>
  AuthenticationProviderContext ProviderContext { get; }

  /// <summary>Gets the validated provider-native identity.</summary>
  ExternalAuthenticatedIdentity ExternalIdentity { get; }
}

/// <summary>
/// Default immutable implementation of the trusted external-authentication feature.
/// </summary>
public sealed class ValidatedExternalAuthenticationFeature
  : IValidatedExternalAuthenticationFeature
{
  /// <summary>Initializes the trusted feature from validated provider context and identity.</summary>
  /// <param name="providerContext">Validated provider-routing context.</param>
  /// <param name="externalIdentity">Validated provider-native identity.</param>
  public ValidatedExternalAuthenticationFeature(
    AuthenticationProviderContext providerContext,
    ExternalAuthenticatedIdentity externalIdentity)
  {
    ArgumentNullException.ThrowIfNull(providerContext);
    ArgumentNullException.ThrowIfNull(externalIdentity);

    ProviderContext = providerContext;
    ExternalIdentity = externalIdentity;
  }

  /// <inheritdoc />
  public AuthenticationProviderContext ProviderContext { get; }

  /// <inheritdoc />
  public ExternalAuthenticatedIdentity ExternalIdentity { get; }
}
