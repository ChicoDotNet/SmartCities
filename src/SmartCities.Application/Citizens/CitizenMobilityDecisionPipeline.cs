using System.Security.Cryptography;
using System.Text;
using SmartCities.Application.HumanOversight;
using SmartCities.Criterion;
using SmartCities.Decisions;
using SmartCities.Evidence;

namespace SmartCities.Application.Citizens;

/// <summary>
/// Builds the deterministic F3 MVP criterion request from an authoritative Evidence Case and ensures its human review is persisted.
/// </summary>
/// <remarks>
/// Replays always start from the authoritative persisted Evidence Case. A retry can therefore repair a missing review,
/// reuse a matching pending review, or preserve a matching finalized review without changing human authority.
/// </remarks>
public sealed class CitizenMobilityDecisionPipeline
  : ICitizenMobilityDecisionPipeline
{
  private readonly ICriterionKernel criterionKernel;
  private readonly IDecisionReviewRepository reviewRepository;

  /// <summary>Initializes the provider-neutral decision pipeline.</summary>
  /// <param name="criterionKernel">Configured criterion provider.</param>
  /// <param name="reviewRepository">Authoritative decision-review repository.</param>
  public CitizenMobilityDecisionPipeline(
    ICriterionKernel criterionKernel,
    IDecisionReviewRepository reviewRepository)
  {
    ArgumentNullException.ThrowIfNull(criterionKernel);
    ArgumentNullException.ThrowIfNull(reviewRepository);

    this.criterionKernel = criterionKernel;
    this.reviewRepository = reviewRepository;
  }

  /// <inheritdoc />
  public async Task<DecisionReview> EnsureReviewAsync(
    EvidenceCase evidenceCase,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(evidenceCase);

    var request = CriterionDecisionRequest.FromEvidenceCase(
      CreateRequestId(evidenceCase.CaseId),
      evidenceCase);

    var trace = await criterionKernel
      .EvaluateAsync(
        request,
        cancellationToken)
      .ConfigureAwait(false);

    ValidateTraceability(request, trace);

    return await reviewRepository
      .GetOrAddPendingAsync(
        DecisionReview.Pending(trace),
        cancellationToken)
      .ConfigureAwait(false);
  }

  private static string CreateRequestId(
    string caseId)
  {
    var digest = SHA256.HashData(
      Encoding.UTF8.GetBytes(caseId));

    return $"mobility:{Convert.ToHexString(digest)}";
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
    var preservesEvidence =
      request.EvidenceReferenceIds.SequenceEqual(
        trace.EvidenceReferenceIds,
        StringComparer.Ordinal);

    if (!preservesRequest
      || !preservesCase
      || !preservesEvidence
      || !trace.RequiresHumanReview)
    {
      throw new InvalidOperationException(
        "Criterion trace must preserve the authoritative request, Evidence Case, evidence identities, and human-review requirement.");
    }
  }
}
