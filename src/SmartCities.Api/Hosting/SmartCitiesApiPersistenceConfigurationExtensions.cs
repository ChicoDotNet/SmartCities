using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Composition;
using SmartCities.Infrastructure.Persistence;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Binds API-host configuration to the provider-neutral SmartCities composition boundary.
/// </summary>
public static class SmartCitiesApiPersistenceConfigurationExtensions
{
  /// <summary>Configuration key selecting the relational provider.</summary>
  public const string DbProviderKey =
    "SmartCities:Persistence:DbProvider";

  /// <summary>Configuration key containing the selected provider's connection string.</summary>
  public const string ConnectionStringKey =
    "SmartCities:Persistence:ConnectionString";

  /// <summary>
  /// Reads and validates the API host's relational persistence configuration.
  /// </summary>
  /// <param name="configuration">Host configuration assembled by ASP.NET Core.</param>
  /// <returns>Validated persistence options suitable for <c>AddSmartCities(...)</c>.</returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="configuration"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when the provider or connection string is missing or the provider name is unsupported.
  /// </exception>
  public static SmartCitiesPersistenceOptions GetSmartCitiesPersistenceOptions(
    this IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    var configuredProvider = configuration[DbProviderKey];

    if (string.IsNullOrWhiteSpace(configuredProvider))
    {
      throw new InvalidOperationException(
        $"Configuration value '{DbProviderKey}' is required.");
    }

    if (!Enum.TryParse<DbProvider>(
        configuredProvider,
        ignoreCase: true,
        out var provider)
      || !Enum.IsDefined(provider))
    {
      throw new InvalidOperationException(
        $"Configuration value '{DbProviderKey}' contains unsupported provider '{configuredProvider}'.");
    }

    var connectionString = configuration[ConnectionStringKey];

    if (string.IsNullOrWhiteSpace(connectionString))
    {
      throw new InvalidOperationException(
        $"Configuration value '{ConnectionStringKey}' is required.");
    }

    return SmartCitiesPersistenceOptions.Create(
      provider,
      connectionString);
  }

  /// <summary>
  /// Registers SmartCities from the API host's validated persistence configuration.
  /// </summary>
  /// <param name="services">Host dependency-injection services.</param>
  /// <param name="configuration">ASP.NET Core host configuration.</param>
  /// <returns>The same service collection for composition chaining.</returns>
  public static IServiceCollection AddSmartCitiesFromConfiguration(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    return services.AddSmartCities(
      configuration.GetSmartCitiesPersistenceOptions());
  }
}
