using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartCities.Identity;

namespace SmartCities.Api.Identity;

/// <summary>
/// Composes ASP.NET Core authentication with the SmartCities provider-adapter canonicalization boundary.
/// </summary>
public static class SmartCitiesAuthenticationCanonicalizationExtensions
{
  /// <summary>
  /// Registers authentication infrastructure and provider-neutral canonicalization without choosing a default scheme.
  /// </summary>
  /// <param name="services">Host dependency-injection services.</param>
  /// <returns>The same service collection for composition chaining.</returns>
  public static IServiceCollection AddSmartCitiesAuthenticationCanonicalization(
    this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddAuthentication();

    services.TryAddScoped<
      IAuthenticationProviderAdapterResolver,
      AuthenticationProviderAdapterResolver>();
    services.TryAddScoped<
      IAuthenticationCanonicalizer,
      AuthenticationCanonicalizer>();

    return services;
  }

  /// <summary>
  /// Adds canonicalization after concrete authentication and before authorization.
  /// </summary>
  /// <param name="app">HTTP pipeline builder.</param>
  /// <returns>The same application builder for pipeline chaining.</returns>
  public static IApplicationBuilder UseSmartCitiesAuthenticationCanonicalization(
    this IApplicationBuilder app)
  {
    ArgumentNullException.ThrowIfNull(app);

    return app.UseMiddleware<
      SmartCitiesAuthenticationCanonicalizationMiddleware>();
  }
}
