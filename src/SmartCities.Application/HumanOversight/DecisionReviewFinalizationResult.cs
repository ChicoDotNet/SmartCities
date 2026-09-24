using SmartCities.Decisions;

namespace SmartCities.Application.HumanOversight;

/// <summary>
/// Carries the authoritative result and review snapshot for a finalization attempt.
/// </summary>
public sealed record DecisionReviewFinalizationResult
{
  private DecisionReviewFinalizationResult(
    DecisionReviewFinalizationOutcome outcome,
    DecisionReview? review)
  {
    Outcome = outcome;
    Review = review;
  }

  /// <summary>Gets the finalization outcome.</summary>
  public DecisionReviewFinalizationOutcome Outcome { get; }

  /// <summary>Gets the authoritative review snapshot when one exists.</summary>
  public DecisionReview? Review { get; }

  /// <summary>Creates a successful finalization result.</summary>
  public static DecisionReviewFinalizationResult Finalized(
    DecisionReview review)
  {
    ArgumentNullException.ThrowIfNull(review);

    if (review.Status != DecisionReviewStatus.Finalized)
    {
      throw new ArgumentException(
        "A finalized result requires a finalized review.",
        nameof(review));
    }

    return new DecisionReviewFinalizationResult(
      DecisionReviewFinalizationOutcome.Finalized,
      review);
  }

  /// <summary>Creates a not-found result.</summary>
  public static DecisionReviewFinalizationResult NotFound() =>
    new(
      DecisionReviewFinalizationOutcome.NotFound,
      review: null);

  /// <summary>Creates an already-finalized result preserving the original authoritative snapshot.</summary>
  public static DecisionReviewFinalizationResult AlreadyFinalized(
    DecisionReview review)
  {
    ArgumentNullException.ThrowIfNull(review);

    if (review.Status != DecisionReviewStatus.Finalized)
    {
      throw new ArgumentException(
        "An already-finalized result requires a finalized review.",
        nameof(review));
    }

    return new DecisionReviewFinalizationResult(
      DecisionReviewFinalizationOutcome.AlreadyFinalized,
      review);
  }
}
