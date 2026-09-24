using SmartCities.Decisions;
using SmartCities.Evidence;

namespace SmartCities.Application.Citizens;

/// <summary>
/// Defines the application boundary that turns an authoritative mobility Evidence Case into accountable human review.
/// </summary>
public interface ICitizenMobilityDecisionPipeline
{
  /// <summary>
  /// Ensures the authoritative Evidence Case has exactly one trace-consistent decision review.
  /// </summary>
  /// <param name="evidenceCase">Authoritative persisted Evidence Case.</param>
  /// <param name="cancellationToken">Token used to cancel the operation.</param>
  /// <returns>The authoritative pending or already-finalized review associated with the case.</returns>
  Task<DecisionReview> EnsureReviewAsync(
    EvidenceCase evidenceCase,
    CancellationToken cancellationToken = default);
}
