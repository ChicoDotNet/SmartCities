using Microsoft.Extensions.DependencyInjection;
using SmartCities.Application.Configuration;
using SmartCities.Application.FeatureFlags;
using SmartCities.Configuration.Sqlite;

namespace SmartCities.Composition;

/// <summary>
/// Registers the per-Town-Hall non-sensitive configuration and feature-management plane.
/// </summary>
public static class SmartCitiesFeatureManagementServiceCollectionExtensions
{
  /// <summary>Registers Town Hall context, SQLite settings, and feature flag services.</summary>
  public static IServiceCollection AddSmartCitiesFeatureManagement(
    this IServiceCollection services,
    SmartCitiesFeatureManagementOptions options)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(options);

    var townHall =
      new TownHallContext(options.TownHallId);
    var store =
      new SqliteNonSensitiveConfigurationStore(
        options.SqliteConnectionString);

    services.AddSingleton(townHall);
    services.AddSingleton<
      INonSensitiveConfigurationStore>(
        store);
    services.AddSingleton<
      IFeatureFlagService,
      FeatureFlagService>();

    return services;
  }
}
