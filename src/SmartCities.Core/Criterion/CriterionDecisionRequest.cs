using System.Collections.ObjectModel;
using SmartCities.Evidence;

namespace SmartCities.Criterion;

/// <summary>
/// Represents a bounded, provider-neutral request for criterion evaluation.
/// </summary>
public sealed record CriterionDecisionRequest
{
  private CriterionDecisionRequest(
    string requestId,
    string? evidenceCaseId,
    string subject,
    IReadOnlyList<string> evidenceReferenceIds)
  {
    RequestId = requestId;
    EvidenceCaseId = evidenceCaseId;
    Subject = subject;
    EvidenceReferenceIds = evidenceReferenceIds;
  }

  /// <summary>Gets the stable identifier of this criterion request.</summary>
  public string RequestId { get; }

  /// <summary>
  /// Gets the originating Evidence Case identifier when this request was derived from a case,
  /// or <see langword="null"/> for a generic criterion request.
  /// </summary>
  public string? EvidenceCaseId { get; }

  /// <summary>Gets the bounded question or subject to evaluate.</summary>
  public string Subject { get; }

  /// <summary>Gets immutable evidence identifiers supplied to the criterion provider.</summary>
  public IReadOnlyList<string> EvidenceReferenceIds { get; }

  /// <summary>Creates a validated generic criterion request.</summary>
  /// <param name="requestId">Stable non-empty request identifier.</param>
  /// <param name="subject">Non-empty bounded subject to evaluate.</param>
  /// <param name="evidenceReferenceIds">Evidence identifiers available to the provider.</param>
  /// <returns>An immutable provider-neutral request with no Evidence Case link.</returns>
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

    var evidence = CopyAndValidateEvidence(evidenceReferenceIds);

    return new CriterionDecisionRequest(
      requestId,
      evidenceCaseId: null,
      subject,
      evidence);
  }

  /// <summary>
  /// Creates a bounded criterion request from an immutable Evidence Case snapshot.
  /// </summary>
  /// <param name="requestId">Stable non-empty request identifier.</param>
  /// <param name="evidenceCase">Evidence Case that supplies the subject and evidence identities.</param>
  /// <returns>
  /// An immutable request linked to the Evidence Case without exposing evidence payloads or provenance internals
  /// to the provider-neutral criterion contract.
  /// </returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="requestId"/> is empty or whitespace.</exception>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="evidenceCase"/> is <see langword="null"/>.</exception>
  public static CriterionDecisionRequest FromEvidenceCase(
    string requestId,
    EvidenceCase evidenceCase)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
    ArgumentNullException.ThrowIfNull(evidenceCase);

    var evidence = CopyAndValidateEvidence(evidenceCase.EvidenceReferenceIds);

    return new CriterionDecisionRequest(
      requestId,
      evidenceCase.CaseId,
      evidenceCase.Subject,
      evidence);
  }

  private static ReadOnlyCollection<string> CopyAndValidateEvidence(
    IEnumerable<string> evidenceReferenceIds)
  {
    var evidence = evidenceReferenceIds.ToArray();

    if (evidence.Any(string.IsNullOrWhiteSpace))
    {
      throw new ArgumentException(
        "Evidence reference identifiers cannot be empty or whitespace.",
        nameof(evidenceReferenceIds));
    }

    return Array.AsReadOnly(evidence);
  }
}
