namespace SmartCities.Identity;

/// <summary>
/// Defines stable authorization policy names consumed by SmartCities API surfaces.
/// </summary>
public static class SmartCitiesPolicies
{
  /// <summary>
  /// Requires an authenticated canonical human authority with permission to finalize a decision review.
  /// </summary>
  public const string FinalizeDecisionReview =
    "smartcities.decision-review.finalize";

  /// <summary>
  /// Requires an authenticated canonical authority with the global operational feature-management grant.
  /// </summary>
  /// <remarks>
  /// A feature-specific manage permission is still required at the operation boundary.
  /// </remarks>
  public const string ManageFeatureFlags =
    "smartcities.feature-flags.manage";

  /// <summary>
  /// Requires Administration admission plus the global permission to configure feature flags.
  /// </summary>
  /// <remarks>
  /// A feature-specific config permission is still required for the target feature.
  /// </remarks>
  public const string ConfigureFeatureFlags =
    "smartcities.feature-flags.config";

  /// <summary>
  /// Requires the global feature-management grant plus the citizen-mobility-specific manage grant.
  /// </summary>
  public const string ManageCitizenMobility =
    "smartcities.citizen-mobility.manage";

  /// <summary>
  /// Requires canonical authentication and admission to Town Hall Administration.
  /// </summary>
  public const string TownHallAdministrationAccess =
    "smartcities.administration.access";

  /// <summary>
  /// Requires Administration admission plus explicit permission to manage whitelist rules.
  /// </summary>
  public const string ManageAdministrationWhitelist =
    "smartcities.administration-whitelist.manage";

  /// <summary>
  /// Requires Administration admission plus explicit permission to manage persisted authorization grants.
  /// </summary>
  public const string ManageAdministrationGrants =
    "smartcities.administration-grants.manage";

  /// <summary>
  /// Requires Administration admission plus explicit permission to read immutable audit history.
  /// </summary>
  public const string ReadAdministrationAudit =
    "smartcities.administration-audit.read";
}
