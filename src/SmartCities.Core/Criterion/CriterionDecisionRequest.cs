namespace SmartCities.Criterion;

/// <summary>
/// Represents a bounded, provider-neutral request for criterion evaluation.
/// </summary>
public sealed record CriterionDecisionRequest
{
  private CriterionDecisionRequest(
    string requestId,
    string subject,
    IReadOnlyList<string> evidenceReferenceIds)
  {
    RequestId = requestId;
    Subject = subject;
    EvidenceReferenceIds = evidenceReferenceIds;
  }

  /// <summary>Gets the stable identifier of this criterion request.</summary>
  public string RequestId { get; }

  /// <summary>Gets the bounded question or subject to evaluate.</summary>
  public string Subject { get; }

  /// <summary>Gets immutable evidence identifiers supplied to the criterion provider.</summary>
  public IReadOnlyList<string> EvidenceReferenceIds { get; }

  /// <summary>Creates a validated criterion request.</summary>
  /// <param name="requestId">Stable non-empty request identifier.</param>
  /// <param name="subject">Non-empty bounded subject to evaluate.</param>
  /// <param name="evidenceReferenceIds">Evidence identifiers available to the provider.</param>
  /// <returns>An immutable provider-neutral request.</returns>
  /// <exception cref="ArgumentException">
  /// Thrown when the request identifier, subject, or any evidence identifier is empty or whitespace.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="evidenceReferenceIds"/> is <see langword="null"/>.
  /// </exception>
  public static CriterionDecisionRequest Create(
    string requestId,
    string subject,
    IEnumerable<string> evidenceReferenceIds)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
    ArgumentException.ThrowIfNullOrWhiteSpace(subject);
    ArgumentNullException.ThrowIfNull(evidenceReferenceIds);

    var evidence = evidenceReferenceIds.ToArray();

    if (evidence.Any(string.IsNullOrWhiteSpace))
    {
      throw new ArgumentException(
        "Evidence reference identifiers cannot be empty or whitespace.",
        nameof(evidenceReferenceIds));
    }

    return new CriterionDecisionRequest(requestId, subject, evidence);
  }
}
