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
  /// Allows an authenticated Town Hall administrator to manage known deployment feature flags.
  /// </summary>
  public const string ManageFeatureFlags =
    "feature-flags.manage";

  /// <summary>
  /// Allows an admitted Town Hall administrator to manage Administration whitelist rules.
  /// </summary>
  public const string ManageAdministrationWhitelist =
    "administration-whitelist.manage";
}
