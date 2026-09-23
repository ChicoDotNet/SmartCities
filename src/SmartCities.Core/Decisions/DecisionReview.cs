namespace SmartCities.Decisions;

/// <summary>
/// Represents the public human-review boundary between a criterion recommendation and a final civic disposition.
/// </summary>
/// <remarks>
/// A recommendation starts pending and cannot become final without an explicit <see cref="HumanAuthority"/>.
/// The type is immutable: finalization returns a new snapshot and never mutates the pending instance.
/// </remarks>
public sealed record DecisionReview
{
  private DecisionReview(
    string recommendationId,
    DecisionReviewStatus status,
    HumanAuthority? authority,
    DecisionDisposition? disposition)
  {
    RecommendationId = recommendationId;
    Status = status;
    Authority = authority;
    Disposition = disposition;
  }

  /// <summary>Gets the stable identifier of the recommendation under review.</summary>
  public string RecommendationId { get; }

  /// <summary>Gets the current human-review lifecycle state.</summary>
  public DecisionReviewStatus Status { get; }

  /// <summary>Gets the accountable human authority after finalization, or <see langword="null"/> while pending.</summary>
  public HumanAuthority? Authority { get; }

  /// <summary>Gets the final disposition after finalization, or <see langword="null"/> while pending.</summary>
  public DecisionDisposition? Disposition { get; }

  /// <summary>Creates a pending human review for a recommendation.</summary>
  /// <param name="recommendationId">Stable non-empty identifier of the recommendation to review.</param>
  /// <returns>A pending immutable review snapshot with no final authority or disposition.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="recommendationId"/> is empty or whitespace.</exception>
  public static DecisionReview Pending(string recommendationId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(recommendationId);

    return new DecisionReview(
      recommendationId,
      DecisionReviewStatus.PendingHumanReview,
      authority: null,
      disposition: null);
  }

  /// <summary>Records the final civic disposition under an explicit human authority.</summary>
  /// <param name="authority">The accountable human authority making the final disposition.</param>
  /// <param name="disposition">The disposition selected by that authority.</param>
  /// <returns>A finalized immutable review snapshot.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="authority"/> is <see langword="null"/>.</exception>
  /// <exception cref="InvalidOperationException">Thrown when this review has already been finalized.</exception>
  public DecisionReview Finalize(HumanAuthority authority, DecisionDisposition disposition)
  {
    ArgumentNullException.ThrowIfNull(authority);

    if (Status == DecisionReviewStatus.Finalized)
    {
      throw new InvalidOperationException("A finalized decision review cannot be finalized again.");
    }

    return new DecisionReview(
      RecommendationId,
      DecisionReviewStatus.Finalized,
      authority,
      disposition);
  }
}
