namespace SmartCities.Identity;

/// <summary>
/// Defines stable SmartCities permissions independently of authentication providers.
/// </summary>
public static class SmartCitiesPermissions
{
  /// <summary>
  /// Allows an authenticated human authority to finalize a pending civic decision review.
  /// </summary>
  public const string FinalizeDecisionReview =
    "decision-review.finalize";

  /// <summary>
  /// Allows an authenticated authority to enter operational feature-management surfaces.
  /// </summary>
  /// <remarks>
  /// This global grant is insufficient by itself. Operational endpoints must also require
  /// the specific feature permission produced by <see cref="SmartCitiesFeaturePermissions.Manage(string)"/>.
  /// </remarks>
  public const string ManageFeatureFlags =
    "feature-flags.manage";

  /// <summary>
  /// Allows an admitted Town Hall administrator to configure registered feature flags.
  /// </summary>
  /// <remarks>
  /// This global grant is insufficient by itself. Configuration also requires the specific
  /// feature permission produced by <see cref="SmartCitiesFeaturePermissions.Configure(string)"/>.
  /// </remarks>
  public const string ConfigureFeatureFlags =
    "feature-flags.config";

  /// <summary>
  /// Allows an admitted Town Hall administrator to manage Administration whitelist rules.
  /// </summary>
  public const string ManageAdministrationWhitelist =
    "administration-whitelist.manage";

  /// <summary>
  /// Allows an admitted Town Hall administrator to assign persisted canonical roles and permissions.
  /// </summary>
  public const string ManageAdministrationGrants =
    "administration-grants.manage";

  /// <summary>
  /// Allows an admitted Town Hall administrator to read immutable control-plane audit history.
  /// </summary>
  public const string ReadAdministrationAudit =
    "administration-audit.read";
}
