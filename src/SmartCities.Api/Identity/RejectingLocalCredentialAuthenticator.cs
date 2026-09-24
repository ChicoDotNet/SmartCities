using SmartCities.Identity;

namespace SmartCities.Api.Identity;

internal sealed class RejectingLocalCredentialAuthenticator
  : ILocalCredentialAuthenticator
{
  public Task<ExternalAuthenticatedIdentity?> AuthenticateAsync(
    string userName,
    string password,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    return Task.FromResult<
      ExternalAuthenticatedIdentity?>(
        null);
  }
}
