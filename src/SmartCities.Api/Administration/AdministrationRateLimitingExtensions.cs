using Microsoft.AspNetCore.RateLimiting;

namespace SmartCities.Api.Administration;

/// <summary>
/// Configures conservative throttling for the temporary bootstrap credential endpoint.
/// </summary>
public static class AdministrationRateLimitingExtensions
{
  /// <summary>Registers the named bootstrap credential rate-limit policy.</summary>
  public static IServiceCollection AddSmartCitiesAdministrationRateLimiting(
    this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddRateLimiter(
      options =>
        options.AddFixedWindowLimiter(
          "administration-bootstrap",
          limiter =>
          {
            limiter.PermitLimit = 5;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
            limiter.AutoReplenishment = true;
          }));

    return services;
  }
}
