using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartCities.Application.FeatureFlags;

namespace SmartCities.Composition;

internal sealed class FeatureManagementHealthCheck
  : IHealthCheck
{
  private readonly IFeatureFlagService features;

  public FeatureManagementHealthCheck(
    IFeatureFlagService features)
  {
    ArgumentNullException.ThrowIfNull(features);
    this.features = features;
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
