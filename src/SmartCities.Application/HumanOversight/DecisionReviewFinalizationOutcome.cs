namespace SmartCities.Application.HumanOversight;

/// <summary>
/// Represents the authoritative result of attempting to finalize a human decision review.
/// </summary>
public enum DecisionReviewFinalizationOutcome
{
  /// <summary>The pending review was finalized by this operation.</summary>
  Finalized = 0,

  /// <summary>No review exists for the supplied recommendation identifier.</summary>
  NotFound = 1,

  /// <summary>The review had already been finalized by another authoritative operation.</summary>
  AlreadyFinalized = 2,
}
