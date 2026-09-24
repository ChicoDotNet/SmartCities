using System.Globalization;

namespace SmartCities.Api.Localization;

/// <summary>
/// Provides API-facing localized resources without exposing resource-manager implementation details.
/// </summary>
public interface IApiLocalizationCatalog
{
  /// <summary>
  /// Returns the complete public API resource bundle for a requested culture.
  /// </summary>
  /// <param name="requestedCulture">Requested culture name, or <see langword="null"/> for neutral English.</param>
  /// <returns>A deterministic resource bundle with the culture actually resolved.</returns>
  ApiLocalizationResourceResponse GetResources(
    string? requestedCulture);

  /// <summary>
  /// Resolves one public API resource for an already-negotiated UI culture.
  /// </summary>
  /// <param name="key">Stable resource key.</param>
  /// <param name="culture">Requested UI culture.</param>
  /// <returns>The localized resource value, using neutral English when the culture is unsupported.</returns>
  string GetString(
    string key,
    CultureInfo culture);
}
