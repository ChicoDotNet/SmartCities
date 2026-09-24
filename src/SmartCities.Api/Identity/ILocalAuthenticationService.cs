using SmartCities.Identity;

namespace SmartCities.Api.Identity;

/// <summary>
/// Orchestrates local credential authentication into canonical session or JWT identities.
/// </summary>
public interface ILocalAuthenticationService
{
  /// <summary>Gets whether the local authentication provider is enabled for this deployment.</summary>
  bool IsEnabled { get; }

  /// <summary>Authenticates credentials and returns the canonical SmartCities identity.</summary>
  Task<CanonicalIdentity?> AuthenticateAsync(
    string userName,
    string password,
    CancellationToken cancellationToken = default);

  /// <summary>Authenticates credentials and, when valid, creates a locally signed bearer token.</summary>
  Task<LocalAccessToken?> IssueAccessTokenAsync(
    string userName,
    string password,
    CancellationToken cancellationToken = default);
}
