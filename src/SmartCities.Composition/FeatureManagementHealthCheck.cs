using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartCities.Application.Administration;
using SmartCities.Application.FeatureFlags;

namespace SmartCities.Composition;

internal sealed class FeatureManagementHealthCheck
  : IHealthCheck
{
  private readonly IFeatureFlagService features;
  private readonly IAdministrationAccessService administration;
  private readonly IAdministrationAuthorizationGrantService grants;
  private readonly IAdministrationControlPlaneAuditService audit;

  public FeatureManagementHealthCheck(
    IFeatureFlagService features,
    IAdministrationAccessService administration,
    IAdministrationAuthorizationGrantService grants,
    IAdministrationControlPlaneAuditService audit)
  {
    ArgumentNullException.ThrowIfNull(features);
    ArgumentNullException.ThrowIfNull(administration);
    ArgumentNullException.ThrowIfNull(grants);
    ArgumentNullException.ThrowIfNull(audit);
    this.features = features;
    this.administration = administration;
    this.grants = grants;
    this.audit = audit;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);

    try
    {
      _ = await features
        .GetAllAsync(cancellationToken)
        .ConfigureAwait(false);
      _ = await administration
        .GetRulesAsync(cancellationToken)
        .ConfigureAwait(false);
      _ = await grants
        .GetAllAsync(cancellationToken)
        .ConfigureAwait(false);
      _ = await audit
        .GetRecentAsync(
          1,
          cancellationToken)
        .ConfigureAwait(false);

      return HealthCheckResult.Healthy();
    }
    catch (Exception exception)
      when (exception is not OperationCanceledException)
    {
      return HealthCheckResult.Unhealthy(
        "Non-sensitive Town Hall configuration is unavailable.");
    }
  }
}
