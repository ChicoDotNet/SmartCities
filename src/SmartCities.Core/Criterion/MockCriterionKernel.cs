namespace SmartCities.Criterion;

/// <summary>
/// Provides deterministic criterion behavior for development, contract tests, and demonstrations before a real provider is configured.
/// </summary>
/// <remarks>
/// This mock does not imitate proprietary reasoning or claim domain expertise. It only exercises the public contract.
/// </remarks>
public sealed class MockCriterionKernel : ICriterionKernel
{
  /// <inheritdoc />
  public Task<CriterionDecisionTrace> EvaluateAsync(
    CriterionDecisionRequest request,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(request);
    cancellationToken.ThrowIfCancellationRequested();

    var trace = CriterionDecisionTrace.Create(
      request.RequestId,
      $"mock:{request.RequestId}",
      CriterionRecommendation.RequiresHumanReview,
      requiresHumanReview: true,
      request.EvidenceReferenceIds,
      "Mock criterion evaluation completed. An accountable human review is required before final civic disposition.",
      evidenceCaseId: request.EvidenceCaseId);

    return Task.FromResult(trace);
  }
}
