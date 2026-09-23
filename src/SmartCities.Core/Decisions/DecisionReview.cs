using SmartCities.Criterion;

namespace SmartCities.Decisions;

/// <summary>
/// Represents the public human-review boundary between a criterion recommendation and a final civic disposition.
/// </summary>
/// <remarks>
/// A recommendation starts pending and cannot become final without an explicit <see cref="HumanAuthority"/>.
/// The type is immutable: finalization returns a new snapshot and never mutates the pending instance.
/// When created from a <see cref="CriterionDecisionTrace"/>, the review retains the complete public origin trace
/// through finalization.
/// </remarks>
public sealed record DecisionReview
{
  private DecisionReview(
    string recommendationId,
    string? criterionRequestId,
    string? evidenceCaseId,
    IReadOnlyList<string> evidenceReferenceIds,
    DecisionReviewStatus status,
    HumanAuthority? authority,
    DecisionDisposition? disposition)
  {
    RecommendationId = recommendationId;
    CriterionRequestId = criterionRequestId;
    EvidenceCaseId = evidenceCaseId;
    EvidenceReferenceIds = evidenceReferenceIds;
    Status = status;
    Authority = authority;
    Disposition = disposition;
  }

  /// <summary>Gets the stable identifier of the recommendation under review.</summary>
  public string RecommendationId { get; }

  /// <summary>
  /// Gets the originating Criterion request identifier when trace-aware review was used,
  /// or <see langword="null"/> for a lightweight generic review.
  /// </summary>
  public string? CriterionRequestId { get; }

  /// <summary>
  /// Gets the originating Evidence Case identifier when available,
  /// or <see langword="null"/> when the recommendation was not case-bound.
  /// </summary>
  public string? EvidenceCaseId { get; }

  /// <summary>
  /// Gets the evidence identifiers retained from the originating Criterion trace.
  /// </summary>
  public IReadOnlyList<string> EvidenceReferenceIds { get; }

  /// <summary>Gets the current human-review lifecycle state.</summary>
  public DecisionReviewStatus Status { get; }

  /// <summary>Gets the accountable human authority after finalization, or <see langword="null"/> while pending.</summary>
  public HumanAuthority? Authority { get; }

  /// <summary>Gets the final disposition after finalization, or <see langword="null"/> while pending.</summary>
  public DecisionDisposition? Disposition { get; }

  /// <summary>Creates a lightweight pending human review for a recommendation identifier.</summary>
  /// <param name="recommendationId">Stable non-empty identifier of the recommendation to review.</param>
  /// <returns>
  /// A pending immutable review snapshot with no Criterion request, Evidence Case, evidence identities,
  /// final authority, or disposition.
  /// </returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="recommendationId"/> is empty or whitespace.</exception>
  public static DecisionReview Pending(string recommendationId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(recommendationId);

    return new DecisionReview(
      recommendationId,
      criterionRequestId: null,
      evidenceCaseId: null,
      Array.Empty<string>(),
      DecisionReviewStatus.PendingHumanReview,
      authority: null,
      disposition: null);
  }

  /// <summary>
  /// Creates a trace-aware pending human review from an advisory Criterion trace.
  /// </summary>
  /// <param name="trace">Criterion trace whose recommendation requires accountable human review.</param>
  /// <returns>
  /// A pending immutable review retaining Criterion request, Evidence Case, recommendation, and evidence identities.
  /// </returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="trace"/> is <see langword="null"/>.</exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when the supplied trace does not require accountable human review.
  /// </exception>
  public static DecisionReview Pending(CriterionDecisionTrace trace)
  {
    ArgumentNullException.ThrowIfNull(trace);

    if (!trace.RequiresHumanReview)
    {
      throw new InvalidOperationException(
        "A Criterion trace that does not require human review cannot enter the pending human-review workflow.");
    }

    return new DecisionReview(
      trace.RecommendationId,
      trace.RequestId,
      trace.EvidenceCaseId,
      Array.AsReadOnly(trace.EvidenceReferenceIds.ToArray()),
      DecisionReviewStatus.PendingHumanReview,
      authority: null,
      disposition: null);
  }

  /// <summary>Records the final civic disposition under an explicit human authority.</summary>
  /// <param name="authority">The accountable human authority making the final disposition.</param>
  /// <param name="disposition">The disposition selected by that authority.</param>
  /// <returns>
  /// A finalized immutable review snapshot retaining all origin traceability captured by the pending review.
  /// </returns>
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
      CriterionRequestId,
      EvidenceCaseId,
      EvidenceReferenceIds,
      DecisionReviewStatus.Finalized,
      authority,
      disposition);
  }
}
