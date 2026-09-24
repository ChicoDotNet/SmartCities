namespace SmartCities.Api.FeatureFlags;

/// <summary>
/// Requests an explicit enablement override for a known feature.
/// </summary>
/// <param name="Enabled">Desired effective feature state.</param>
public sealed record SetFeatureFlagRequest(
  bool Enabled);
