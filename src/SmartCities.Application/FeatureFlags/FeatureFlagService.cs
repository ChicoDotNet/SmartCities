using SmartCities.Application.Administration;
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
        [SmartCitiesFeatures.UrbanAccessibility] = false,
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

      if (state is null)
      {
        throw new InvalidOperationException(
          $"Registered feature '{featureId}' could not be resolved.");
      }

      result.Add(state);
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
  public Task<FeatureFlagState?> SetAsync(
    string featureId,
    bool enabled,
    CancellationToken cancellationToken = default) =>
    SetCoreAsync(
      featureId,
      enabled,
      auditContext: null,
      cancellationToken);

  /// <inheritdoc />
  public Task<FeatureFlagState?> SetAsync(
    string featureId,
    bool enabled,
    AdministrationControlPlaneAuditContext auditContext,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(auditContext);

    return SetCoreAsync(
      featureId,
      enabled,
      auditContext,
      cancellationToken);
  }

  private async Task<FeatureFlagState?> SetCoreAsync(
    string featureId,
    bool enabled,
    AdministrationControlPlaneAuditContext? auditContext,
    CancellationToken cancellationToken)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      featureId);

    if (!Definitions.ContainsKey(featureId))
    {
      return null;
    }

    var value = enabled ? "true" : "false";

    if (auditContext is null)
    {
      await store
        .SetAsync(
          TownHallId,
          SettingKey(featureId),
          value,
          cancellationToken)
        .ConfigureAwait(false);
    }
    else
    {
      var previous = await GetAsync(
          featureId,
          cancellationToken)
        .ConfigureAwait(false);

      if (previous is null)
      {
        return null;
      }

      var auditEvent =
        AdministrationControlPlaneAuditEvent.CreateMutation(
          TownHallId,
          auditContext,
          AdministrationAuditActions.FeatureFlagSet,
          AdministrationAuditResourceTypes.FeatureFlag,
          featureId,
          featureId,
          previous.Enabled ? "true" : "false",
          value);

      await store
        .SetAsync(
          TownHallId,
          SettingKey(featureId),
          value,
          auditEvent,
          cancellationToken)
        .ConfigureAwait(false);
    }

    return new FeatureFlagState(
      featureId,
      enabled);
  }


  private static string SettingKey(
    string featureId) =>
    $"{SettingPrefix}{featureId}:enabled";
}
