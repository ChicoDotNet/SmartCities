namespace SmartCities.Criterion;

/// <summary>Represents the advisory recommendation state returned by a criterion provider.</summary>
public enum CriterionRecommendation
{
  /// <summary>
  /// The provider has produced an advisory trace that requires accountable human review before any final civic disposition.
  /// </summary>
  RequiresHumanReview = 0,
}
