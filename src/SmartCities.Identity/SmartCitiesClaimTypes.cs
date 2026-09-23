namespace SmartCities.Identity;

/// <summary>
/// Defines canonical SmartCities claim types consumed by authorization and audit boundaries.
/// </summary>
/// <remarks>
/// Authentication-provider adapters must normalize external claims into these identifiers. Application code must not
/// depend directly on provider-specific claim names.
/// </remarks>
public static class SmartCitiesClaimTypes
{
  /// <summary>
  /// Globally stable SmartCities subject identifier after provider/tenant normalization.
  /// </summary>
  public const string Subject =
    "urn:smartcities:identity:subject";

  /// <summary>
  /// Human authority context under which the subject may act.
  /// </summary>
  public const string AuthorityRole =
    "urn:smartcities:identity:authority-role";

  /// <summary>
  /// Explicit capability granted to the canonical subject.
  /// </summary>
  public const string Permission =
    "urn:smartcities:identity:permission";

  /// <summary>
  /// Identity-source identifier retained for audit and diagnostics, not as an authorization grant by itself.
  /// </summary>
  public const string IdentityProvider =
    "urn:smartcities:identity:provider";
}
