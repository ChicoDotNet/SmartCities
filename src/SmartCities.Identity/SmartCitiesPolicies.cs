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
}
