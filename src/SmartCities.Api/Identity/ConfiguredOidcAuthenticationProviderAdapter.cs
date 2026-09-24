using SmartCities.Identity;

namespace SmartCities.Api.Identity;

internal sealed class ConfiguredOidcAuthenticationProviderAdapter
  : IAuthenticationProviderAdapter
{
  private readonly OidcAuthenticationProviderConfiguration configuration;

  internal ConfiguredOidcAuthenticationProviderAdapter(
    OidcAuthenticationProviderConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    this.configuration = configuration;
  }

  public string ProviderId =>
    configuration.ProviderId;

  public bool CanHandle(
    AuthenticationProviderContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return string.Equals(
        context.AuthenticationScheme,
        configuration.SchemeName,
        StringComparison.Ordinal)
      && string.Equals(
        NormalizeIssuer(context.Issuer),
        configuration.Authority,
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

    AddMappedValues(
      identity,
      configuration.RoleClaimType,
      configuration.RoleMappings,
      roles);
    AddMappedValues(
      identity,
      configuration.PermissionClaimType,
      configuration.PermissionMappings,
      permissions);

    var tenant = string.IsNullOrWhiteSpace(
      context.TenantId)
        ? "default"
        : context.TenantId;

    return Task.FromResult(
      CanonicalIdentity.Create(
        ProviderId,
        $"{ProviderId}:{tenant}:{identity.Subject}",
        roles,
        permissions));
  }

  private static void AddMappedValues(
    ExternalAuthenticatedIdentity identity,
    string? claimType,
    IReadOnlyDictionary<string, string> mappings,
    HashSet<string> destination)
  {
    if (claimType is null || mappings.Count == 0)
    {
      return;
    }

    foreach (var claim in identity.Claims.Where(
      claim => string.Equals(
        claim.Type,
        claimType,
        StringComparison.Ordinal)))
    {
      if (mappings.TryGetValue(
        claim.Value,
        out var canonical))
      {
        destination.Add(canonical);
      }
    }
  }

  private static string NormalizeIssuer(
    string issuer) =>
    issuer.Trim().TrimEnd('/');
}
