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
}
