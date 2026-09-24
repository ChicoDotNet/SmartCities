using SmartCities.Application.Configuration;
using SmartCities.Application.FeatureFlags;
using Xunit;

namespace SmartCities.Application.Tests.FeatureFlags;

public sealed class FeatureFlagServiceTests
{
  [Fact]
  public async Task Missing_persisted_value_uses_the_registered_default()
  {
    var store = new RecordingConfigurationStore();
    var service = new FeatureFlagService(
      new TownHallContext("town-hall-a"),
      store);

    var state = await service.GetAsync(
      SmartCitiesFeatures.CitizenMobility,
      TestContext.Current.CancellationToken);

    Assert.NotNull(state);
    Assert.True(state.Enabled);
    Assert.Equal(
      SmartCitiesFeatures.CitizenMobility,
      state.FeatureId);
  }

  [Fact]
  public async Task Persisted_values_are_isolated_by_town_hall()
  {
    var store = new RecordingConfigurationStore();
    var first = new FeatureFlagService(
      new TownHallContext("town-hall-a"),
      store);
    var second = new FeatureFlagService(
      new TownHallContext("town-hall-b"),
      store);

    await first.SetAsync(
      SmartCitiesFeatures.CitizenMobility,
      enabled: false,
      TestContext.Current.CancellationToken);

    var firstState = await first.GetAsync(
      SmartCitiesFeatures.CitizenMobility,
      TestContext.Current.CancellationToken);
    var secondState = await second.GetAsync(
      SmartCitiesFeatures.CitizenMobility,
      TestContext.Current.CancellationToken);

    Assert.NotNull(firstState);
    Assert.False(firstState.Enabled);
    Assert.NotNull(secondState);
    Assert.True(secondState.Enabled);
  }

  [Fact]
  public async Task Unknown_feature_cannot_be_persisted()
  {
    var service = new FeatureFlagService(
      new TownHallContext("town-hall-a"),
      new RecordingConfigurationStore());

    var state = await service.SetAsync(
      "unknown-feature",
      enabled: true,
      TestContext.Current.CancellationToken);

    Assert.Null(state);
  }

  private sealed class RecordingConfigurationStore
    : INonSensitiveConfigurationStore
  {
    private readonly Dictionary<string, string> values =
      new(StringComparer.Ordinal);

    public Task<string?> GetAsync(
      string townHallId,
      string key,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      values.TryGetValue(
        $"{townHallId}\u001f{key}",
        out var value);

      return Task.FromResult(value);
    }

    public Task SetAsync(
      string townHallId,
      string key,
      string value,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      values[$"{townHallId}\u001f{key}"] = value;
      return Task.CompletedTask;
    }
  }
}
