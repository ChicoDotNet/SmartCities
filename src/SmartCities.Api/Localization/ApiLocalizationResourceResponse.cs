namespace SmartCities.Api.Localization;

/// <summary>
/// Represents the localized resource payload consumed by API clients such as the React application.
/// </summary>
/// <param name="RequestedCulture">Culture requested by the client.</param>
/// <param name="ResolvedCulture">Supported culture actually used to resolve values.</param>
/// <param name="Resources">Stable resource keys and localized values.</param>
public sealed record ApiLocalizationResourceResponse(
  string RequestedCulture,
  string ResolvedCulture,
  IReadOnlyDictionary<string, string> Resources);
