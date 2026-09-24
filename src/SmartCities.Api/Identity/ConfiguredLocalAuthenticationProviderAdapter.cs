using SmartCities.Identity;

namespace SmartCities.Api.Identity;

internal sealed class ConfiguredLocalAuthenticationProviderAdapter
  : IAuthenticationProviderAdapter
{
  private readonly LocalAuthenticationProviderConfiguration configuration;

  internal ConfiguredLocalAuthenticationProviderAdapter(
    LocalAuthenticationProviderConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    this.configuration = configuration;
  }

  public string ProviderId => "local";

  public bool CanHandle(
    AuthenticationProviderContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return string.Equals(
        context.AuthenticationScheme,
        SmartCitiesAuthenticationSchemes.LocalJwt,
        StringComparison.Ordinal)
      && string.Equals(
        context.Issuer,
        configuration.Issuer,
        StringComparison.Ordinal);
  }

  public Task<CanonicalIdentity> NormalizeAsync(
    AuthenticationProviderContext context,
    ExternalAuthenticatedIdentity identity,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(identity);
    cancellationToken.ThrowIfCancellationRequested();

    var roles = new HashSet<string>(
      configuration.DefaultAuthorityRoles,
      StringComparer.Ordinal);
    var permissions = new HashSet<string>(
      configuration.DefaultPermissions,
      StringComparer.Ordinal);

    foreach (var claim in identity.Claims)
    {
      if (string.Equals(
          claim.Type,
          configuration.RoleClaimType,
          StringComparison.Ordinal)
        && configuration.AllowedAuthorityRoles.Contains(
          claim.Value))
      {
        roles.Add(claim.Value);
      }

      if (string.Equals(
          claim.Type,
          configuration.PermissionClaimType,
          StringComparison.Ordinal)
        && configuration.AllowedPermissions.Contains(
          claim.Value))
      {
        permissions.Add(claim.Value);
      }
    }

    return Task.FromResult(
      CanonicalIdentity.Create(
        ProviderId,
        $"local:default:{identity.Subject}",
        roles.Order(StringComparer.Ordinal),
        permissions.Order(StringComparer.Ordinal),
        CanonicalEmailClaimResolver.Resolve(
          identity,
          configuration.EmailClaimType,
          verificationClaimType: null,
          requireVerified: false)));
  }
}
