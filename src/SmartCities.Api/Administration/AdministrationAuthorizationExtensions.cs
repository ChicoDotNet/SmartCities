using Microsoft.AspNetCore.Authorization;

namespace SmartCities.Api.Administration;

/// <summary>
/// Registers the runtime handler that evaluates Town Hall Administration whitelist admission.
/// </summary>
public static class AdministrationAuthorizationExtensions
{
  /// <summary>Registers the Administration admission authorization handler.</summary>
  public static IServiceCollection AddSmartCitiesAdministrationAuthorization(
    this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddSingleton<
      IAuthorizationHandler,
      AdministrationAccessAuthorizationHandler>();

    return services;
  }
}
