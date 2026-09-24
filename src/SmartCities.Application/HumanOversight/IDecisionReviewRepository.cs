using SmartCities.Decisions;

namespace SmartCities.Application.HumanOversight;

/// <summary>
/// Defines authoritative persistence operations for human decision reviews.
/// </summary>
public interface IDecisionReviewRepository
{
  /// <summary>Persists a pending decision review.</summary>
  Task AddPendingAsync(
    DecisionReview review,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the authoritative review for the recommendation or atomically creates the supplied pending review when absent.
  /// </summary>
  Task<DecisionReview> GetOrAddPendingAsync(
    DecisionReview review,
    CancellationToken cancellationToken = default);

  /// <summary>Gets the authoritative review linked to an Evidence Case identifier.</summary>
  Task<DecisionReview?> GetByEvidenceCaseIdAsync(
    string evidenceCaseId,
    CancellationToken cancellationToken = default);

  /// <summary>Gets the authoritative review by recommendation identifier.</summary>
  Task<DecisionReview?> GetAsync(
    string recommendationId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Atomically finalizes a pending review without allowing a later reviewer to overwrite the first disposition.
  /// </summary>
  Task<DecisionReviewFinalizationResult> FinalizeAsync(
    string recommendationId,
    HumanAuthority authority,
    DecisionDisposition disposition,
    CancellationToken cancellationToken = default);
}
