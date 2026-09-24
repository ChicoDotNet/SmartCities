namespace SmartCities.Identity;

/// <summary>
/// Validates local credentials against a deployment-owned credential store.
/// </summary>
/// <remarks>
/// Implementations must never log or persist the supplied password. A successful result represents an already
/// authenticated local external identity that still passes through SmartCities canonicalization.
/// </remarks>
public interface ILocalCredentialAuthenticator
{
  /// <summary>
  /// Authenticates local credentials and returns the validated provider-native identity, or <see langword="null"/>.
  /// </summary>
  /// <param name="userName">Local account name or login identifier.</param>
  /// <param name="password">Secret presented by the caller.</param>
  /// <param name="cancellationToken">Token used to cancel credential-store work.</param>
  Task<ExternalAuthenticatedIdentity?> AuthenticateAsync(
    string userName,
    string password,
    CancellationToken cancellationToken = default);
}
