namespace SmartCities.Criterion;

/// <summary>
/// Represents the public, provider-neutral trace returned by a criterion evaluation.
/// </summary>
/// <remarks>
/// A criterion trace is advisory. It is not a final civic disposition and does not carry human authority.
/// </remarks>
public sealed record CriterionDecisionTrace
{
  private CriterionDecisionTrace(
    string requestId,
    string? evidenceCaseId,
    string recommendationId,
    CriterionRecommendation recommendation,
    bool requiresHumanReview,
    IReadOnlyList<string> evidenceReferenceIds,
    string publicExplanation)
  {
    RequestId = requestId;
    EvidenceCaseId = evidenceCaseId;
    RecommendationId = recommendationId;
    Recommendation = recommendation;
    RequiresHumanReview = requiresHumanReview;
    EvidenceReferenceIds = evidenceReferenceIds;
    PublicExplanation = publicExplanation;
  }

  /// <summary>Gets the request identifier evaluated by the provider.</summary>
  public string RequestId { get; }

  /// <summary>
  /// Gets the originating Evidence Case identifier when the request was case-bound,
  /// or <see langword="null"/> for a generic request.
  /// </summary>
  public string? EvidenceCaseId { get; }

  /// <summary>Gets the stable identifier of the provider recommendation.</summary>
  public string RecommendationId { get; }

  /// <summary>Gets the advisory recommendation state.</summary>
  public CriterionRecommendation Recommendation { get; }

  /// <summary>Gets a value indicating whether accountable human review is required.</summary>
  public bool RequiresHumanReview { get; }

  /// <summary>Gets the evidence identifiers consumed or carried by the trace.</summary>
  public IReadOnlyList<string> EvidenceReferenceIds { get; }

  /// <summary>Gets a public-safe explanation suitable for later presentation or localization layers.</summary>
  public string PublicExplanation { get; }

  /// <summary>Creates a validated advisory criterion trace.</summary>
  /// <param name="requestId">Identifier of the evaluated request.</param>
  /// <param name="recommendationId">Stable recommendation identifier.</param>
  /// <param name="recommendation">Advisory recommendation state.</param>
  /// <param name="requiresHumanReview">Whether human review is required before final disposition.</param>
  /// <param name="evidenceReferenceIds">Evidence identifiers represented by the trace.</param>
  /// <param name="publicExplanation">Non-empty public-safe explanation.</param>
  /// <param name="evidenceCaseId">Optional originating Evidence Case identifier.</param>
  /// <returns>An immutable criterion trace.</returns>
  public static CriterionDecisionTrace Create(
    string requestId,
    string recommendationId,
    CriterionRecommendation recommendation,
    bool requiresHumanReview,
    IEnumerable<string> evidenceReferenceIds,
    string publicExplanation,
    string? evidenceCaseId = null)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
    ArgumentException.ThrowIfNullOrWhiteSpace(recommendationId);
    ArgumentNullException.ThrowIfNull(evidenceReferenceIds);
    ArgumentException.ThrowIfNullOrWhiteSpace(publicExplanation);

    if (evidenceCaseId is not null)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(evidenceCaseId);
    }

    var evidence = evidenceReferenceIds.ToArray();

    if (evidence.Any(string.IsNullOrWhiteSpace))
    {
      throw new ArgumentException(
        "Evidence reference identifiers cannot be empty or whitespace.",
        nameof(evidenceReferenceIds));
    }

    return new CriterionDecisionTrace(
      requestId,
      evidenceCaseId,
      recommendationId,
      recommendation,
      requiresHumanReview,
      Array.AsReadOnly(evidence),
      publicExplanation);
  }
}
