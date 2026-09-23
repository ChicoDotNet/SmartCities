using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Registers the SmartCities API controller boundary and its deterministic input-error semantics.
/// </summary>
public static class SmartCitiesApiControllerExtensions
{
  /// <summary>
  /// Adds SmartCities controllers with stable citizen-input validation and domain-error translation.
  /// </summary>
  /// <param name="services">Host service collection.</param>
  /// <returns>The MVC builder for further host composition.</returns>
  public static IMvcBuilder AddSmartCitiesApiControllers(
    this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    return services
      .AddControllers(options =>
        options.Filters.Add<InvalidCitizenInputExceptionFilter>())
      .ConfigureApiBehaviorOptions(options =>
      {
        options.InvalidModelStateResponseFactory = context =>
        {
          var fields = context.ModelState
            .Where(static entry => entry.Value?.Errors.Count > 0)
            .Select(static entry => entry.Key);

          return new BadRequestObjectResult(
            CitizenInputProblemDetails.Create(fields));
        };
      });
  }
}
