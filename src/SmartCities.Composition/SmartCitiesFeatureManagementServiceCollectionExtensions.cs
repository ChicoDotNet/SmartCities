using Microsoft.Extensions.DependencyInjection;
using SmartCities.Application.Administration;
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
    services.AddSingleton(townHall);
    services.AddSingleton<
      INonSensitiveConfigurationStore>(
        _ =>
          new SqliteNonSensitiveConfigurationStore(
            options.SqliteConnectionString));
    services.AddSingleton<
      IFeatureFlagService,
      FeatureFlagService>();
    services.AddSingleton<
      IAdministrationAccessRuleStore>(
        _ =>
          new SqliteAdministrationAccessRuleStore(
            options.SqliteConnectionString));
    services.AddSingleton<
      IAdministrationAccessService,
      AdministrationAccessService>();
    services.AddSingleton<
      IAdministrationAuthorizationGrantStore>(
        _ =>
          new SqliteAdministrationAuthorizationGrantStore(
            options.SqliteConnectionString));
    services.AddSingleton<
      IAdministrationAuthorizationGrantCatalog,
      SmartCitiesAdministrationAuthorizationGrantCatalog>();
    services.AddSingleton<
      IAdministrationAuthorizationGrantService,
      AdministrationAuthorizationGrantService>();

    services
      .AddHealthChecks()
      .AddCheck<FeatureManagementHealthCheck>(
        name: "configuration",
        tags: ["ready"]);

    return services;
  }
}
