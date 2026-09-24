using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Api.Hosting;
using SmartCities.Application.Configuration;
using SmartCities.Application.FeatureFlags;
using SmartCities.Configuration.Sqlite;
using Xunit;

namespace SmartCities.Api.Tests.Hosting;

public sealed class SmartCitiesApiFeatureManagementConfigurationTests
{
  [Fact]
  public void Host_configuration_registers_the_Town_Hall_feature_control_plane()
  {
    var configuration = BuildConfiguration(
      "ags-town-hall",
      "Data Source=:memory:");
    var services = new ServiceCollection();

    services.AddSmartCitiesFeatureManagementFromConfiguration(
      configuration);

    using var provider = services.BuildServiceProvider();

    var context = provider
      .GetRequiredService<TownHallContext>();
    var store = provider
      .GetRequiredService<INonSensitiveConfigurationStore>();
    var features = provider
      .GetRequiredService<IFeatureFlagService>();

    Assert.Equal(
      "ags-town-hall",
      context.TownHallId);
    Assert.Equal(
      "ags-town-hall",
      features.TownHallId);
    Assert.IsType<
      SqliteNonSensitiveConfigurationStore>(
        store);
  }

  [Fact]
  public void Host_configuration_rejects_a_missing_Town_Hall_identifier()
  {
    var configuration = BuildConfiguration(
      townHallId: null,
      connectionString: "Data Source=config.db");

    var exception =
      Assert.Throws<InvalidOperationException>(
        () => configuration
          .GetSmartCitiesFeatureManagementOptions());

    Assert.Contains(
      "SmartCities:TownHall:Id",
      exception.Message,
      StringComparison.Ordinal);
  }

  [Fact]
  public void Host_configuration_rejects_a_missing_configuration_connection_string()
  {
    var configuration = BuildConfiguration(
      "ags-town-hall",
      connectionString: null);

    var exception =
      Assert.Throws<InvalidOperationException>(
        () => configuration
          .GetSmartCitiesFeatureManagementOptions());

    Assert.Contains(
      "SmartCities:Configuration:ConnectionString",
      exception.Message,
      StringComparison.Ordinal);
  }

  private static IConfiguration BuildConfiguration(
    string? townHallId,
    string? connectionString)
  {
    var values =
      new Dictionary<string, string?>();

    if (townHallId is not null)
    {
      values[
        "SmartCities:TownHall:Id"] =
        townHallId;
    }

    if (connectionString is not null)
    {
      values[
        "SmartCities:Configuration:ConnectionString"] =
        connectionString;
    }

    return new ConfigurationBuilder()
      .AddInMemoryCollection(values)
      .Build();
  }
}
