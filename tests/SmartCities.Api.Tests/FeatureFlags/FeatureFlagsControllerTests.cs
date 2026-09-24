using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Api.Citizens;
using SmartCities.Api.FeatureFlags;
using SmartCities.Application.FeatureFlags;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Api.Tests.FeatureFlags;

public sealed class FeatureFlagsControllerTests
{
  [Fact]
  public async Task Public_snapshot_exposes_only_known_feature_states_for_the_current_town_hall()
  {
    var service = new RecordingFeatureFlagService(
      "town-hall-a",
      [
        new FeatureFlagState(
          SmartCitiesFeatures.CitizenMobility,
          Enabled: false),
      ]);
    var controller = CreateController(service);

    var result = await controller.GetAsync(
      TestContext.Current.CancellationToken);

    var payload = Assert.IsType<FeatureFlagSnapshotResponse>(
      Assert.IsType<OkObjectResult>(result.Result).Value);

    Assert.Equal("town-hall-a", payload.TownHallId);
    var feature = Assert.Single(payload.Features);
    Assert.Equal(
      SmartCitiesFeatures.CitizenMobility,
      feature.FeatureId);
    Assert.False(feature.Enabled);
    Assert.Equal(
      "no-store",
      controller.Response.Headers.CacheControl.ToString());
  }

  [Theory]
  [InlineData(typeof(CitizenMobilityReportsController))]
  [InlineData(typeof(CitizenMobilityOutcomesController))]
  public void Citizen_mobility_HTTP_surfaces_are_gated_by_the_registered_vertical_slice(
    Type controllerType)
  {
    var gate = Assert.Single(
      controllerType.GetCustomAttributes(
        typeof(RequireFeatureAttribute),
        inherit: true)
      .Cast<RequireFeatureAttribute>());

    Assert.Equal(
      SmartCitiesFeatures.CitizenMobility,
      gate.FeatureId);
  }

  [Fact]
  public void Mutation_requires_the_canonical_feature_management_policy()
  {
    var method = typeof(FeatureFlagsController)
      .GetMethod(nameof(FeatureFlagsController.SetAsync));

    var authorize = Assert.Single(
      method!.GetCustomAttributes(
        typeof(AuthorizeAttribute),
        inherit: true)
      .Cast<AuthorizeAttribute>());

    Assert.Equal(
      SmartCitiesPolicies.ManageFeatureFlags,
      authorize.Policy);
  }

  [Fact]
  public async Task Unknown_feature_returns_404_instead_of_creating_ad_hoc_flags()
  {
    var controller = CreateController(
      new RecordingFeatureFlagService(
        "town-hall-a",
        []));

    var result = await controller.SetAsync(
      "unknown-feature",
      new SetFeatureFlagRequest(Enabled: true),
      TestContext.Current.CancellationToken);

    Assert.IsType<NotFoundResult>(result.Result);
  }

  private static FeatureFlagsController CreateController(
    IFeatureFlagService service) =>
    new(service)
    {
      ControllerContext =
        new ControllerContext
        {
          HttpContext =
            new DefaultHttpContext(),
        },
    };

  private sealed class RecordingFeatureFlagService
    : IFeatureFlagService
  {
    private readonly IReadOnlyList<FeatureFlagState> states;

    public RecordingFeatureFlagService(
      string townHallId,
      IReadOnlyList<FeatureFlagState> states)
    {
      TownHallId = townHallId;
      this.states = states;
    }

    public string TownHallId { get; }

    public Task<IReadOnlyList<FeatureFlagState>> GetAllAsync(
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(states);
    }

    public Task<FeatureFlagState?> GetAsync(
      string featureId,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(
        states.SingleOrDefault(
          item => string.Equals(
            item.FeatureId,
            featureId,
            StringComparison.Ordinal)));
    }

    public Task<FeatureFlagState?> SetAsync(
      string featureId,
      bool enabled,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var existing = states.SingleOrDefault(
        item => string.Equals(
          item.FeatureId,
          featureId,
          StringComparison.Ordinal));

      return Task.FromResult(
        existing is null
          ? null
          : existing with { Enabled = enabled });
    }
  }
}
