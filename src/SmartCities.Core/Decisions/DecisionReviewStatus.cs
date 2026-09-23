namespace SmartCities.Decisions;

/// <summary>Represents the lifecycle state of a human review for a criterion recommendation.</summary>
public enum DecisionReviewStatus
{
  /// <summary>The recommendation is waiting for an accountable human authority.</summary>
  PendingHumanReview = 0,

  /// <summary>An accountable human authority recorded the final civic disposition.</summary>
  Finalized = 1,
}
