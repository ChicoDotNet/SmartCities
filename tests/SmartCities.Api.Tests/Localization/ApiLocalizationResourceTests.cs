using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartCities.Api.Hosting;
using SmartCities.Api.Localization;
using Xunit;

namespace SmartCities.Api.Tests.Localization;

public sealed class ApiLocalizationResourceTests
{
  [Fact]
  public void Catalog_returns_neutral_english_resources()
  {
    var catalog = new ResxApiLocalizationCatalog();

    var bundle = catalog.GetResources("en");

    Assert.Equal("en", bundle.RequestedCulture);
    Assert.Equal("en", bundle.ResolvedCulture);
    Assert.Equal(
      "Invalid citizen input.",
      bundle.Resources["problem.invalidInput.title"]);
    Assert.Equal(
      "One or more supplied values are invalid.",
      bundle.Resources["problem.invalidInput.detail"]);
  }

  [Fact]
  public void Catalog_returns_es_mx_resources()
  {
    var catalog = new ResxApiLocalizationCatalog();

    var bundle = catalog.GetResources("es-MX");

    Assert.Equal("es-MX", bundle.RequestedCulture);
    Assert.Equal("es-MX", bundle.ResolvedCulture);
    Assert.Equal(
      "Datos ciudadanos no válidos.",
      bundle.Resources["problem.invalidInput.title"]);
    Assert.Equal(
      "Uno o más valores proporcionados no son válidos.",
      bundle.Resources["problem.invalidInput.detail"]);
  }

  [Fact]
  public void Unsupported_culture_falls_back_deterministically_to_neutral_english()
  {
    var catalog = new ResxApiLocalizationCatalog();

    var bundle = catalog.GetResources("fr-FR");

    Assert.Equal("fr-FR", bundle.RequestedCulture);
    Assert.Equal("en", bundle.ResolvedCulture);
    Assert.Equal(
      "Invalid citizen input.",
      bundle.Resources["problem.invalidInput.title"]);
  }

  [Fact]
  public void Localization_controller_exposes_the_resx_bundle_for_react_clients()
  {
    var controller = new LocalizationResourcesController(
      new ResxApiLocalizationCatalog());

    var result = controller.Get("es-MX");

    var response = Assert.IsType<OkObjectResult>(result.Result);
    var payload = Assert.IsType<ApiLocalizationResourceResponse>(
      response.Value);

    Assert.Equal("es-MX", payload.RequestedCulture);
    Assert.Equal("es-MX", payload.ResolvedCulture);
    Assert.Equal(
      "Datos ciudadanos no válidos.",
      payload.Resources["problem.invalidInput.title"]);
  }

  [Fact]
  public void Invalid_input_problem_details_use_the_negotiated_es_mx_ui_culture()
  {
    var services = new ServiceCollection();
    services.AddSmartCitiesApiControllers();

    using var provider = services.BuildServiceProvider();
    var options = provider
      .GetRequiredService<IOptions<ApiBehaviorOptions>>()
      .Value;

    var modelState = new ModelStateDictionary();
    modelState.AddModelError(
      "Description",
      "The Description field is required.");

    var httpContext = new DefaultHttpContext
    {
      RequestServices = provider,
    };

    httpContext.Features.Set<IRequestCultureFeature>(
      new RequestCultureFeature(
        new RequestCulture("es-MX"),
        provider: null));

    var actionContext = new ActionContext(
      httpContext,
      new RouteData(),
      new ActionDescriptor(),
      modelState);

    var result = options.InvalidModelStateResponseFactory(actionContext);
    var response = Assert.IsType<BadRequestObjectResult>(result);
    var problem = Assert.IsType<ProblemDetails>(response.Value);

    Assert.Equal("Datos ciudadanos no válidos.", problem.Title);
    Assert.Equal(
      "Uno o más valores proporcionados no son válidos.",
      problem.Detail);
    Assert.Equal("invalid_input", problem.Extensions["code"]);
  }
}
