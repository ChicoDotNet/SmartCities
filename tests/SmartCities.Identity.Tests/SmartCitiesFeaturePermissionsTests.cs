using SmartCities.Identity;
using Xunit;

namespace SmartCities.Identity.Tests;

public sealed class SmartCitiesFeaturePermissionsTests
{
  [Fact]
  public void Feature_permissions_keep_operation_and_configuration_distinct()
  {
    Assert.Equal(
      "citizen-mobility.manage",
      SmartCitiesFeaturePermissions.Manage(
        "citizen-mobility"));
    Assert.Equal(
      "citizen-mobility.config",
      SmartCitiesFeaturePermissions.Configure(
        "citizen-mobility"));
    Assert.NotEqual(
      SmartCitiesFeaturePermissions.Manage(
        "citizen-mobility"),
      SmartCitiesFeaturePermissions.Configure(
        "citizen-mobility"));
  }

  [Theory]
  [InlineData("")]
  [InlineData("Citizen Mobility")]
  [InlineData("citizen/mobility")]
  [InlineData("citizen_mobility")]
  [InlineData(".citizen-mobility")]
  public void Feature_permission_builder_rejects_non_canonical_feature_ids(
    string featureId)
  {
    Assert.Throws<ArgumentException>(
      () => SmartCitiesFeaturePermissions.Manage(
        featureId));
    Assert.Throws<ArgumentException>(
      () => SmartCitiesFeaturePermissions.Configure(
        featureId));
  }
}
