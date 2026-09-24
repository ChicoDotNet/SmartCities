using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SmartCities.Identity;

namespace SmartCities.Api.Identity;

internal sealed class LocalAuthenticationService
  : ILocalAuthenticationService
{
  private readonly AuthenticationProviderConfigurationSet configured;
  private readonly ILocalCredentialAuthenticator credentialAuthenticator;
  private readonly IAuthenticationCanonicalizer canonicalizer;
  private readonly TimeProvider timeProvider;

  public LocalAuthenticationService(
    AuthenticationProviderConfigurationSet configured,
    ILocalCredentialAuthenticator credentialAuthenticator,
    IAuthenticationCanonicalizer canonicalizer,
    TimeProvider timeProvider)
  {
    ArgumentNullException.ThrowIfNull(configured);
    ArgumentNullException.ThrowIfNull(credentialAuthenticator);
    ArgumentNullException.ThrowIfNull(canonicalizer);
    ArgumentNullException.ThrowIfNull(timeProvider);

    this.configured = configured;
    this.credentialAuthenticator = credentialAuthenticator;
    this.canonicalizer = canonicalizer;
    this.timeProvider = timeProvider;
  }

  public bool IsEnabled =>
    configured.Local is not null;

  public async Task<CanonicalIdentity?> AuthenticateAsync(
    string userName,
    string password,
    CancellationToken cancellationToken = default)
  {
    var authenticated = await AuthenticateCoreAsync(
        userName,
        password,
        cancellationToken)
      .ConfigureAwait(false);

    return authenticated?.Canonical;
  }

  public async Task<LocalAccessToken?> IssueAccessTokenAsync(
    string userName,
    string password,
    CancellationToken cancellationToken = default)
  {
    var authenticated = await AuthenticateCoreAsync(
        userName,
        password,
        cancellationToken)
      .ConfigureAwait(false);

    if (authenticated is null)
    {
      return null;
    }

    var local = configured.Local
      ?? throw new InvalidOperationException(
        "Local authentication is not enabled.");

    var now = timeProvider.GetUtcNow();
    var expiresAt = now.AddMinutes(
      local.JwtLifetimeMinutes);

    var claims = new List<Claim>
    {
      new(
        local.SubjectClaimType,
        authenticated.External.Subject),
    };

    claims.AddRange(
      authenticated.Canonical.AuthorityRoles.Select(
        role =>
          new Claim(
            local.RoleClaimType,
            role)));

    claims.AddRange(
      authenticated.Canonical.Permissions.Select(
        permission =>
          new Claim(
            local.PermissionClaimType,
            permission)));

    var descriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Issuer = local.Issuer,
      Audience = local.Audience,
      IssuedAt = now.UtcDateTime,
      NotBefore = now.UtcDateTime,
      Expires = expiresAt.UtcDateTime,
      SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(
            local.SigningKey)),
        SecurityAlgorithms.HmacSha256),
    };

    var handler = new JsonWebTokenHandler();
    var value = handler.CreateToken(descriptor);
    var expiresInSeconds = checked(
      (int)Math.Floor(
        (expiresAt - now).TotalSeconds));

    return new LocalAccessToken(
      value,
      expiresInSeconds);
  }

  private async Task<AuthenticatedLocalIdentity?> AuthenticateCoreAsync(
    string userName,
    string password,
    CancellationToken cancellationToken)
  {
    if (!IsEnabled)
    {
      throw new InvalidOperationException(
        "Local authentication is not enabled.");
    }

    if (string.IsNullOrWhiteSpace(userName)
      || string.IsNullOrWhiteSpace(password))
    {
      return null;
    }

    var local = configured.Local!;
    var external = await credentialAuthenticator
      .AuthenticateAsync(
        userName,
        password,
        cancellationToken)
      .ConfigureAwait(false);

    if (external is null)
    {
      return null;
    }

    var providerContext =
      AuthenticationProviderContext.Create(
        SmartCitiesAuthenticationSchemes.LocalJwt,
        local.Issuer,
        tenantId: null);
    var canonical = await canonicalizer
      .CanonicalizeAsync(
        providerContext,
        external,
        cancellationToken)
      .ConfigureAwait(false);

    return new AuthenticatedLocalIdentity(
      external,
      canonical);
  }

  private sealed record AuthenticatedLocalIdentity(
    ExternalAuthenticatedIdentity External,
    CanonicalIdentity Canonical);
}
