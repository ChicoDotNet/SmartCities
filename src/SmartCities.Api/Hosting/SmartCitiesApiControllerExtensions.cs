using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Api.Localization;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Registers the SmartCities API controller boundary and its deterministic input-error semantics.
/// </summary>
public static class SmartCitiesApiControllerExtensions
{
  /// <summary>
  /// Adds SmartCities controllers, localization resources, and stable citizen-input error semantics.
  /// </summary>
  /// <param name="services">Host service collection.</param>
  /// <returns>The MVC builder for further host composition.</returns>
  public static IMvcBuilder AddSmartCitiesApiControllers(
    this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddSingleton<
      IApiLocalizationCatalog,
      ResxApiLocalizationCatalog>();

    services.Configure<RequestLocalizationOptions>(
      options =>
      {
        var supportedCultures =
          new[]
          {
            ResxApiLocalizationCatalog.NeutralEnglishCulture,
            ResxApiLocalizationCatalog.MexicanSpanishCulture,
          };

        options.DefaultRequestCulture =
          new RequestCulture(
            ResxApiLocalizationCatalog.NeutralEnglishCulture);
        options.SetDefaultCulture(
          ResxApiLocalizationCatalog.NeutralEnglishCulture);
        options.AddSupportedCultures(supportedCultures);
        options.AddSupportedUICultures(supportedCultures);
      });

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

          var catalog = context.HttpContext.RequestServices
              ?.GetService<IApiLocalizationCatalog>()
            ?? new ResxApiLocalizationCatalog();

          return new BadRequestObjectResult(
            CitizenInputProblemDetails.Create(
              fields,
              catalog,
              ApiRequestCulture.GetUiCulture(
                context.HttpContext)));
        };
      });
  }
}
