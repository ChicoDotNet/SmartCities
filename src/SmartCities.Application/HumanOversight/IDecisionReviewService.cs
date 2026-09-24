using SmartCities.Decisions;

namespace SmartCities.Application.HumanOversight;

/// <summary>
/// Defines application use cases for accountable human review.
/// </summary>
public interface IDecisionReviewService
{
  /// <summary>Gets the authoritative review snapshot.</summary>
  Task<DecisionReview?> GetAsync(
    string recommendationId,
    CancellationToken cancellationToken = default);

  /// <summary>Finalizes a pending review under an explicit accountable human authority.</summary>
  Task<DecisionReviewFinalizationResult> FinalizeAsync(
    string recommendationId,
    HumanAuthority authority,
    DecisionDisposition disposition,
    CancellationToken cancellationToken = default);
}
