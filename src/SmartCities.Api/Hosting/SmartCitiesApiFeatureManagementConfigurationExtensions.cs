using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Composition;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Binds API-host configuration to the dedicated Town Hall feature-management control plane.
/// </summary>
public static class SmartCitiesApiFeatureManagementConfigurationExtensions
{
  /// <summary>Configuration key containing the stable Town Hall deployment identifier.</summary>
  public const string TownHallIdKey =
    "SmartCities:TownHall:Id";

  /// <summary>Configuration key containing the dedicated non-sensitive SQLite connection string.</summary>
  public const string ConfigurationConnectionStringKey =
    "SmartCities:Configuration:ConnectionString";

  /// <summary>Reads and validates Town Hall feature-management options.</summary>
  public static SmartCitiesFeatureManagementOptions
    GetSmartCitiesFeatureManagementOptions(
      this IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    var townHallId =
      configuration[TownHallIdKey];

    if (string.IsNullOrWhiteSpace(townHallId))
    {
      throw new InvalidOperationException(
        $"Configuration value '{TownHallIdKey}' is required.");
    }

    var connectionString =
      configuration[
        ConfigurationConnectionStringKey];

    if (string.IsNullOrWhiteSpace(connectionString))
    {
      throw new InvalidOperationException(
        $"Configuration value '{ConfigurationConnectionStringKey}' is required.");
    }

    return SmartCitiesFeatureManagementOptions.Create(
      townHallId,
      connectionString);
  }

  /// <summary>Registers the Town Hall feature-management control plane from host configuration.</summary>
  public static IServiceCollection
    AddSmartCitiesFeatureManagementFromConfiguration(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    return services.AddSmartCitiesFeatureManagement(
      configuration
        .GetSmartCitiesFeatureManagementOptions());
  }
}
