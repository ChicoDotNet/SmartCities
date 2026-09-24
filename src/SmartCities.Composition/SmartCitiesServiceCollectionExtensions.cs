using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartCities.Application.Citizens;
using SmartCities.Application.HumanOversight;
using SmartCities.Infrastructure.Persistence;
using SmartCities.Infrastructure.Persistence.Citizens;
using SmartCities.Infrastructure.Persistence.HumanOversight;
using SmartCities.Infrastructure.PostgreSql.Persistence;
using SmartCities.Infrastructure.SqlServer.Persistence;

namespace SmartCities.Composition;

/// <summary>
/// Provides the host-facing dependency composition boundary for SmartCities.
/// </summary>
public static class SmartCitiesServiceCollectionExtensions
{
  /// <summary>
  /// Registers SmartCities application services, repositories, and the selected relational provider.
  /// </summary>
  /// <param name="services">Host service collection.</param>
  /// <param name="persistenceOptions">Validated relational persistence configuration.</param>
  /// <returns>The same service collection for composition chaining.</returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="services"/> or <paramref name="persistenceOptions"/> is <see langword="null"/>.
  /// </exception>
  public static IServiceCollection AddSmartCities(
    this IServiceCollection services,
    SmartCitiesPersistenceOptions persistenceOptions)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(persistenceOptions);

    services.AddDbContext<SmartCitiesDbContext>(
      builder => ConfigureProvider(builder, persistenceOptions));

    services.AddScoped<
      ICitizenMobilityReportRepository,
      EfCitizenMobilityReportRepository>();

    services.AddScoped<
      ICitizenMobilityReportService,
      CitizenMobilityReportService>();

    services.AddScoped<
      IDecisionReviewRepository,
      EfDecisionReviewRepository>();

    services.AddScoped<
      IDecisionReviewService,
      DecisionReviewService>();

    services
      .AddHealthChecks()
      .AddDbContextCheck<SmartCitiesDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);

    return services;
  }

  private static void ConfigureProvider(
    DbContextOptionsBuilder builder,
    SmartCitiesPersistenceOptions options)
  {
    switch (options.Provider)
    {
      case DbProvider.SqlServer:
        SqlServerPersistence.Configure(
          builder,
          options.ConnectionString);
        break;

      case DbProvider.PostgreSql:
        PostgreSqlPersistence.Configure(
          builder,
          options.ConnectionString);
        break;

      default:
        throw new ArgumentOutOfRangeException(
          nameof(options),
          options.Provider,
          "Unsupported SmartCities relational provider.");
    }
  }
}
