using SmartCities.Application.Configuration;

namespace SmartCities.Application.FeatureFlags;

/// <summary>
/// Resolves registered feature defaults plus per-Town-Hall persisted overrides.
/// </summary>
public sealed class FeatureFlagService
  : IFeatureFlagService
{
  private const string SettingPrefix =
    "feature:";

  private static readonly IReadOnlyDictionary<
    string,
    bool> Definitions =
      new Dictionary<string, bool>(
        StringComparer.Ordinal)
      {
        [SmartCitiesFeatures.CitizenMobility] = true,
      };

  private readonly TownHallContext townHall;
  private readonly INonSensitiveConfigurationStore store;

  /// <summary>Initializes the feature service.</summary>
  public FeatureFlagService(
    TownHallContext townHall,
    INonSensitiveConfigurationStore store)
  {
    ArgumentNullException.ThrowIfNull(townHall);
    ArgumentNullException.ThrowIfNull(store);

    this.townHall = townHall;
    this.store = store;
  }

  /// <inheritdoc />
  public string TownHallId =>
    townHall.TownHallId;

  /// <inheritdoc />
  public async Task<IReadOnlyList<FeatureFlagState>>
    GetAllAsync(
      CancellationToken cancellationToken = default)
  {
    var result =
      new List<FeatureFlagState>(
        Definitions.Count);

    foreach (var featureId in Definitions.Keys
      .Order(StringComparer.Ordinal))
    {
      var state = await GetAsync(
          featureId,
          cancellationToken)
        .ConfigureAwait(false);

      result.Add(state!);
    }

    return result;
  }

  /// <inheritdoc />
  public async Task<FeatureFlagState?> GetAsync(
    string featureId,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      featureId);

    if (!Definitions.TryGetValue(
        featureId,
        out var defaultEnabled))
    {
      return null;
    }

    var persisted = await store
      .GetAsync(
        TownHallId,
        SettingKey(featureId),
        cancellationToken)
      .ConfigureAwait(false);

    if (persisted is null)
    {
      return new FeatureFlagState(
        featureId,
        defaultEnabled);
    }

    if (!bool.TryParse(
        persisted,
        out var enabled))
    {
      throw new InvalidOperationException(
        $"Persisted feature flag '{featureId}' for Town Hall '{TownHallId}' is not a valid Boolean value.");
    }

    return new FeatureFlagState(
      featureId,
      enabled);
  }

  /// <inheritdoc />
  public async Task<FeatureFlagState?> SetAsync(
    string featureId,
    bool enabled,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      featureId);

    if (!Definitions.ContainsKey(featureId))
    {
      return null;
    }

    await store
      .SetAsync(
        TownHallId,
        SettingKey(featureId),
        enabled ? "true" : "false",
        cancellationToken)
      .ConfigureAwait(false);

    return new FeatureFlagState(
      featureId,
      enabled);
  }

  private static string SettingKey(
    string featureId) =>
    $"{SettingPrefix}{featureId}:enabled";
}
