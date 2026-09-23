namespace SmartCities.Criterion;

/// <summary>
/// Defines the provider-neutral criterion evaluation boundary used by SmartCities.
/// </summary>
public interface ICriterionKernel
{
  /// <summary>Evaluates a bounded request and returns an advisory, traceable result.</summary>
  /// <param name="request">The request to evaluate.</param>
  /// <param name="cancellationToken">Token used to cancel the operation.</param>
  /// <returns>An advisory criterion trace that still requires the configured human-authority workflow.</returns>
  Task<CriterionDecisionTrace> EvaluateAsync(
    CriterionDecisionRequest request,
    CancellationToken cancellationToken = default);
}
