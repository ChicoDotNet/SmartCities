namespace SmartCities.Application.FeatureFlags;

/// <summary>
/// Represents the effective enablement state of one known vertical slice.
/// </summary>
/// <param name="FeatureId">Stable machine feature identifier.</param>
/// <param name="Enabled">Whether the vertical slice is enabled for the current Town Hall.</param>
public sealed record FeatureFlagState(
  string FeatureId,
  bool Enabled);
