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
  /// Requires an authenticated canonical Town Hall authority with permission to manage feature flags.
  /// </summary>
  public const string ManageFeatureFlags =
    "smartcities.feature-flags.manage";

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
}
