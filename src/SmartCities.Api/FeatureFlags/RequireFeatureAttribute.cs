using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Application.FeatureFlags;

namespace SmartCities.Api.FeatureFlags;

/// <summary>
/// Prevents an HTTP surface from executing when its registered vertical slice is disabled.
/// </summary>
[AttributeUsage(
  AttributeTargets.Class | AttributeTargets.Method,
  AllowMultiple = true,
  Inherited = true)]
public sealed class RequireFeatureAttribute
  : Attribute,
    IAsyncResourceFilter
{
  private readonly string featureId;

  /// <summary>Initializes a feature gate for a stable registered feature identifier.</summary>
  public RequireFeatureAttribute(
    string featureId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      featureId);
    this.featureId = featureId;
  }

  /// <summary>Gets the stable registered feature identifier enforced by this gate.</summary>
  public string FeatureId => featureId;

  /// <inheritdoc />
  public async Task OnResourceExecutionAsync(
    ResourceExecutingContext context,
    ResourceExecutionDelegate next)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(next);

    var service = context.HttpContext.RequestServices
      .GetRequiredService<IFeatureFlagService>();

    var state = await service
      .GetAsync(
        featureId,
        context.HttpContext.RequestAborted)
      .ConfigureAwait(false);

    if (state is null
      || !state.Enabled)
    {
      context.Result =
        new NotFoundResult();
      return;
    }

    await next()
      .ConfigureAwait(false);
  }
}
