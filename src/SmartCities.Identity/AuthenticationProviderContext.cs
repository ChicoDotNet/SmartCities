namespace SmartCities.Identity;

/// <summary>
/// Describes the validated authentication-provider context presented to SmartCities canonicalization.
/// </summary>
/// <remarks>
/// Credential/token validation happens before this boundary. Adapters use this context to decide whether the
/// validated issuer/tenant belongs to them.
/// </remarks>
public sealed record AuthenticationProviderContext
{
  private AuthenticationProviderContext(
    string authenticationScheme,
    string issuer,
    string? tenantId)
  {
    AuthenticationScheme = authenticationScheme;
    Issuer = issuer;
    TenantId = tenantId;
  }

  /// <summary>Gets the host authentication scheme that validated the external identity.</summary>
  public string AuthenticationScheme { get; }

  /// <summary>Gets the validated external issuer identifier.</summary>
  public string Issuer { get; }

  /// <summary>Gets the optional validated tenant/realm identifier.</summary>
  public string? TenantId { get; }

  /// <summary>Creates a validated provider-routing context.</summary>
  /// <param name="authenticationScheme">Authentication scheme that validated the credential.</param>
  /// <param name="issuer">Validated issuer identifier.</param>
  /// <param name="tenantId">Optional validated tenant, realm, or directory identifier.</param>
  /// <returns>An immutable provider context.</returns>
  public static AuthenticationProviderContext Create(
    string authenticationScheme,
    string issuer,
    string? tenantId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      authenticationScheme);
    ArgumentException.ThrowIfNullOrWhiteSpace(issuer);

    var normalizedTenant = string.IsNullOrWhiteSpace(tenantId)
      ? null
      : tenantId.Trim();

    return new AuthenticationProviderContext(
      authenticationScheme.Trim(),
      issuer.Trim(),
      normalizedTenant);
  }
}
