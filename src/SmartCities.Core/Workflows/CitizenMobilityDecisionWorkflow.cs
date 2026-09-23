using SmartCities.Citizens;
using SmartCities.Criterion;
using SmartCities.Decisions;

namespace SmartCities.Workflows;

/// <summary>
/// Composes the pure in-memory mobility decision path from a citizen report to pending human review.
/// </summary>
/// <remarks>
/// This workflow performs no persistence, transport, identity resolution, localization, or transaction management.
/// Those responsibilities belong to later application and infrastructure layers.
/// </remarks>
public static class CitizenMobilityDecisionWorkflow
{
  /// <summary>
  /// Evaluates a validated citizen mobility report and returns the complete in-memory traceability snapshot.
  /// </summary>
  /// <param name="report">Validated citizen mobility report.</param>
  /// <param name="caseId">Stable Evidence Case identifier allocated by the caller.</param>
  /// <param name="requestId">Stable Criterion request identifier allocated by the caller.</param>
  /// <param name="criterionKernel">Configured provider-neutral Criterion Kernel.</param>
  /// <param name="cancellationToken">Token used to cancel Criterion evaluation.</param>
  /// <returns>
  /// A snapshot containing the report, Evidence Case, Criterion request, Criterion trace, and pending human review.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="report"/> or <paramref name="criterionKernel"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// Thrown when <paramref name="caseId"/> or <paramref name="requestId"/> is empty or whitespace.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when the Criterion provider breaks the request, Evidence Case, evidence identity, or human-review traceability contract.
  /// </exception>
  public static async Task<CitizenMobilityDecisionSnapshot> EvaluateAsync(
    CitizenMobilityReport report,
    string caseId,
    string requestId,
    ICriterionKernel criterionKernel,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(report);
    ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
    ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
    ArgumentNullException.ThrowIfNull(criterionKernel);

    var evidenceCase = report.CreateEvidenceCase(caseId);
    var criterionRequest = CriterionDecisionRequest.FromEvidenceCase(
      requestId,
      evidenceCase);

    var criterionTrace = await criterionKernel
      .EvaluateAsync(criterionRequest, cancellationToken)
      .ConfigureAwait(false);

    ValidateTraceability(criterionRequest, criterionTrace);

    var humanReview = DecisionReview.Pending(
      criterionTrace.RecommendationId);

    return new CitizenMobilityDecisionSnapshot(
      report,
      evidenceCase,
      criterionRequest,
      criterionTrace,
      humanReview);
  }

  private static void ValidateTraceability(
    CriterionDecisionRequest request,
    CriterionDecisionTrace trace)
  {
    ArgumentNullException.ThrowIfNull(trace);

    var preservesRequest = string.Equals(
      request.RequestId,
      trace.RequestId,
      StringComparison.Ordinal);

    var preservesCase = string.Equals(
      request.EvidenceCaseId,
      trace.EvidenceCaseId,
      StringComparison.Ordinal);

    var preservesEvidence = request.EvidenceReferenceIds.SequenceEqual(
      trace.EvidenceReferenceIds,
      StringComparer.Ordinal);

    if (!preservesRequest || !preservesCase || !preservesEvidence)
    {
      throw new InvalidOperationException(
        "Criterion trace does not preserve the originating request, Evidence Case, and evidence identities.");
    }

    if (!trace.RequiresHumanReview)
    {
      throw new InvalidOperationException(
        "Citizen mobility Criterion traces must require accountable human review before final civic disposition.");
    }
  }
}
