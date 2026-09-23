using Microsoft.AspNetCore.Mvc;

namespace SmartCities.Api.Localization;

/// <summary>
/// Exposes public localization resources for API clients.
/// </summary>
[ApiController]
[Route("api/localization/resources")]
public sealed class LocalizationResourcesController : ControllerBase
{
  private readonly IApiLocalizationCatalog catalog;

  /// <summary>Initializes the localization endpoint with the API resource catalog.</summary>
  /// <param name="catalog">API localization catalog.</param>
  public LocalizationResourcesController(
    IApiLocalizationCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    this.catalog = catalog;
  }

  /// <summary>
  /// Returns the complete public API resource bundle for the requested culture.
  /// </summary>
  /// <param name="culture">Culture requested by the client; unsupported values fall back to neutral English.</param>
  /// <returns>Localized resources and the culture that was actually resolved.</returns>
  [HttpGet("{culture?}")]
  [ProducesResponseType<ApiLocalizationResourceResponse>(
    StatusCodes.Status200OK)]
  public ActionResult<ApiLocalizationResourceResponse> Get(
    string? culture)
  {
    return Ok(catalog.GetResources(culture));
  }
}
