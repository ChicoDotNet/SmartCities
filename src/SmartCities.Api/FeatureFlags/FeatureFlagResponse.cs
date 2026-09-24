namespace SmartCities.Api.FeatureFlags;

/// <summary>
/// Represents one public non-sensitive feature flag state.
/// </summary>
/// <param name="FeatureId">Stable machine feature identifier.</param>
/// <param name="Enabled">Whether the vertical slice is enabled for the current Town Hall.</param>
public sealed record FeatureFlagResponse(
  string FeatureId,
  bool Enabled);
