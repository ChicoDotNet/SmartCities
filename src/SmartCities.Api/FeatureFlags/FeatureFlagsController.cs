using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Application.FeatureFlags;
using SmartCities.Identity;

namespace SmartCities.Api.FeatureFlags;

/// <summary>
/// Exposes non-sensitive per-Town-Hall feature discovery and protected feature management.
/// </summary>
[ApiController]
[Route("api/system/features")]
public sealed class FeatureFlagsController
  : ControllerBase
{
  private readonly IFeatureFlagService service;

  /// <summary>Initializes the feature-flag HTTP boundary.</summary>
  public FeatureFlagsController(
    IFeatureFlagService service)
  {
    ArgumentNullException.ThrowIfNull(service);
    this.service = service;
  }

  /// <summary>Returns all known effective feature states for the current Town Hall.</summary>
  [HttpGet]
  [ProducesResponseType<FeatureFlagSnapshotResponse>(
    StatusCodes.Status200OK)]
  public async Task<ActionResult<FeatureFlagSnapshotResponse>>
    GetAsync(
      CancellationToken cancellationToken)
  {
    Response.Headers.CacheControl =
      "no-store";

    var states = await service
      .GetAllAsync(cancellationToken)
      .ConfigureAwait(false);

    return Ok(
      new FeatureFlagSnapshotResponse(
        service.TownHallId,
        states
          .Select(
            static state =>
              new FeatureFlagResponse(
                state.FeatureId,
                state.Enabled))
          .ToArray()));
  }

  /// <summary>Persists an enablement override for one known feature.</summary>
  [HttpPut("{featureId}")]
  [Authorize(
    Policy = SmartCitiesPolicies.ConfigureFeatureFlags)]
  [ProducesResponseType<FeatureFlagResponse>(
    StatusCodes.Status200OK)]
  [ProducesResponseType(
    StatusCodes.Status404NotFound)]
  public async Task<ActionResult<FeatureFlagResponse>>
    SetAsync(
      string featureId,
      [FromBody] SetFeatureFlagRequest request,
      CancellationToken cancellationToken)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      featureId);
    ArgumentNullException.ThrowIfNull(request);

    Response.Headers.CacheControl =
      "no-store";

    var current = await service
      .GetAsync(
        featureId,
        cancellationToken)
      .ConfigureAwait(false);

    if (current is null)
    {
      return NotFound();
    }

    var requiredPermission =
      SmartCitiesFeaturePermissions.Configure(
        current.FeatureId);

    if (!User.HasClaim(
        SmartCitiesClaimTypes.Permission,
        requiredPermission))
    {
      return Forbid();
    }

    var state = await service
      .SetAsync(
        current.FeatureId,
        request.Enabled,
        cancellationToken)
      .ConfigureAwait(false);

    if (state is null)
    {
      throw new InvalidOperationException(
        $"Registered feature '{current.FeatureId}' disappeared while applying configuration.");
    }

    return Ok(
      new FeatureFlagResponse(
        state.FeatureId,
        state.Enabled));
  }
}
