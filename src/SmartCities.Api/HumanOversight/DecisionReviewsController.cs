using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Api.Identity;
using SmartCities.Application.HumanOversight;
using SmartCities.Identity;

namespace SmartCities.Api.HumanOversight;

/// <summary>
/// Exposes accountable human-review transitions over HTTP.
/// </summary>
[ApiController]
[Route("api/human-oversight/decision-reviews")]
public sealed class DecisionReviewsController : ControllerBase
{
  private readonly IDecisionReviewService service;

  /// <summary>Initializes the protected human-review controller.</summary>
  public DecisionReviewsController(
    IDecisionReviewService service)
  {
    ArgumentNullException.ThrowIfNull(service);
    this.service = service;
  }

  /// <summary>Finalizes a pending review under the authenticated canonical human authority.</summary>
  [HttpPost("{recommendationId}/finalize")]
  [Authorize(
    Policy = SmartCitiesPolicies.ManageCitizenMobility)]
  [Authorize(
    Policy = SmartCitiesPolicies.FinalizeDecisionReview)]
  [ProducesResponseType<DecisionReviewResponse>(
    StatusCodes.Status200OK)]
  [ProducesResponseType(
    StatusCodes.Status404NotFound)]
  [ProducesResponseType<DecisionReviewResponse>(
    StatusCodes.Status409Conflict)]
  public async Task<ActionResult<DecisionReviewResponse>> FinalizeAsync(
    string recommendationId,
    [FromBody] FinalizeDecisionReviewRequest request,
    CancellationToken cancellationToken)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      recommendationId);
    ArgumentNullException.ThrowIfNull(request);

    var authority = User.ToHumanAuthority(
      request.AuthorityRole);

    var result = await service.FinalizeAsync(
        recommendationId,
        authority,
        request.Disposition,
        cancellationToken)
      .ConfigureAwait(false);

    return result.Outcome switch
    {
      DecisionReviewFinalizationOutcome.Finalized =>
        Ok(
          DecisionReviewResponse.FromDomain(
            result.Review!)),
      DecisionReviewFinalizationOutcome.NotFound =>
        NotFound(),
      DecisionReviewFinalizationOutcome.AlreadyFinalized =>
        Conflict(
          DecisionReviewResponse.FromDomain(
            result.Review!)),
      _ => throw new InvalidOperationException(
        "Unsupported decision-review finalization outcome."),
    };
  }
}
