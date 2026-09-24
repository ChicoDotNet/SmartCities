namespace SmartCities.Api.FeatureFlags;

/// <summary>
/// Represents the public non-sensitive feature snapshot for the current Town Hall deployment.
/// </summary>
/// <param name="TownHallId">Stable Town Hall deployment identifier.</param>
/// <param name="Features">Known feature states.</param>
public sealed record FeatureFlagSnapshotResponse(
  string TownHallId,
  IReadOnlyList<FeatureFlagResponse> Features);
