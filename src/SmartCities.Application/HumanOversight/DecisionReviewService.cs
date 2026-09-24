using SmartCities.Decisions;

namespace SmartCities.Application.HumanOversight;

/// <summary>
/// Coordinates accountable human-review use cases over the authoritative repository boundary.
/// </summary>
public sealed class DecisionReviewService
  : IDecisionReviewService
{
  private readonly IDecisionReviewRepository repository;

  /// <summary>Initializes the service with its authoritative review repository.</summary>
  public DecisionReviewService(
    IDecisionReviewRepository repository)
  {
    ArgumentNullException.ThrowIfNull(repository);
    this.repository = repository;
  }

  /// <inheritdoc />
  public Task<DecisionReview?> GetAsync(
    string recommendationId,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(recommendationId);

    return repository.GetAsync(
      recommendationId,
      cancellationToken);
  }

  /// <inheritdoc />
  public Task<DecisionReviewFinalizationResult> FinalizeAsync(
    string recommendationId,
    HumanAuthority authority,
    DecisionDisposition disposition,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(recommendationId);
    ArgumentNullException.ThrowIfNull(authority);

    if (!Enum.IsDefined(disposition))
    {
      throw new ArgumentOutOfRangeException(
        nameof(disposition),
        disposition,
        "Unsupported decision disposition.");
    }

    return repository.FinalizeAsync(
      recommendationId,
      authority,
      disposition,
      cancellationToken);
  }
}
