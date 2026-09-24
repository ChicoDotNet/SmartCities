using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Application.Citizens;
using SmartCities.Application.Configuration;
using SmartCities.Application.FeatureFlags;
using SmartCities.Application.HumanOversight;
using SmartCities.Composition;
using SmartCities.Criterion;
using SmartCities.Infrastructure.Persistence;
using Xunit;

namespace SmartCities.Composition.Tests.Persistence;

public sealed class SmartCitiesCompositionTests
{
  [Theory]
  [InlineData(DbProvider.SqlServer, "Microsoft.EntityFrameworkCore.SqlServer", "SmartCities.Infrastructure.SqlServer")]
  [InlineData(DbProvider.PostgreSql, "Npgsql.EntityFrameworkCore.PostgreSQL", "SmartCities.Infrastructure.PostgreSql")]
  public void Composition_selects_the_requested_provider_and_its_migration_assembly(
    DbProvider provider,
    string expectedProviderName,
    string expectedMigrationsAssembly)
  {
    var services = new ServiceCollection();

    services.AddSmartCities(
      SmartCitiesPersistenceOptions.Create(
        provider,
        ConnectionStringFor(provider)));

    using var serviceProvider = services.BuildServiceProvider();
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SmartCitiesDbContext>();

    Assert.Equal(expectedProviderName, context.Database.ProviderName);
    Assert.Equal(
      expectedMigrationsAssembly,
      context.GetService<IMigrationsAssembly>().Assembly.GetName().Name);
  }

  [Theory]
  [InlineData(DbProvider.SqlServer)]
  [InlineData(DbProvider.PostgreSql)]
  public void Composition_registers_service_repository_and_db_context_as_scoped(
    DbProvider provider)
  {
    var services = new ServiceCollection();

    services.AddSmartCities(
      SmartCitiesPersistenceOptions.Create(
        provider,
        ConnectionStringFor(provider)));

    Assert.Contains(
      services,
      descriptor =>
        descriptor.ServiceType == typeof(ICitizenMobilityDecisionPipeline)
        && descriptor.ImplementationType == typeof(CitizenMobilityDecisionPipeline)
        && descriptor.Lifetime == ServiceLifetime.Scoped);

    Assert.Contains(
      services,
      descriptor =>
        descriptor.ServiceType == typeof(ICitizenMobilityReportService)
        && descriptor.ImplementationType == typeof(CitizenMobilityReportService)
        && descriptor.Lifetime == ServiceLifetime.Scoped);

    Assert.Contains(
      services,
      descriptor =>
        descriptor.ServiceType == typeof(ICitizenMobilityOutcomeService)
        && descriptor.ImplementationType == typeof(CitizenMobilityOutcomeService)
        && descriptor.Lifetime == ServiceLifetime.Scoped);

    Assert.Contains(
      services,
      descriptor =>
        descriptor.ServiceType == typeof(ICitizenMobilityReportRepository)
        && descriptor.ImplementationType?.Name == "EfCitizenMobilityReportRepository"
        && descriptor.Lifetime == ServiceLifetime.Scoped);

    Assert.Contains(
      services,
      descriptor =>
        descriptor.ServiceType == typeof(IDecisionReviewService)
        && descriptor.ImplementationType == typeof(DecisionReviewService)
        && descriptor.Lifetime == ServiceLifetime.Scoped);

    Assert.Contains(
      services,
      descriptor =>
        descriptor.ServiceType == typeof(ICriterionKernel)
        && descriptor.ImplementationType == typeof(MockCriterionKernel)
        && descriptor.Lifetime == ServiceLifetime.Singleton);

    Assert.Contains(
      services,
      descriptor =>
        descriptor.ServiceType == typeof(IDecisionReviewRepository)
        && descriptor.ImplementationType?.Name == "EfDecisionReviewRepository"
        && descriptor.Lifetime == ServiceLifetime.Scoped);

    Assert.Contains(
      services,
      descriptor =>
        descriptor.ServiceType == typeof(SmartCitiesDbContext)
        && descriptor.Lifetime == ServiceLifetime.Scoped);
  }

  [Fact]
  public void Composition_registers_per_Town_Hall_feature_management_separately_from_domain_persistence()
  {
    var services = new ServiceCollection();

    services.AddSmartCitiesFeatureManagement(
      SmartCitiesFeatureManagementOptions.Create(
        "town-hall-a",
        "Data Source=:memory:"));

    using var provider = services.BuildServiceProvider();

    Assert.Equal(
      "town-hall-a",
      provider
        .GetRequiredService<TownHallContext>()
        .TownHallId);
    Assert.IsType<FeatureFlagService>(
      provider.GetRequiredService<
        IFeatureFlagService>());
    Assert.NotNull(
      provider.GetRequiredService<
        INonSensitiveConfigurationStore>());
    Assert.DoesNotContain(
      services,
      descriptor =>
        descriptor.ServiceType
        == typeof(SmartCitiesDbContext));
  }

  [Fact]
  public void Composition_registers_the_deterministic_mock_criterion_kernel()
  {
    var services = new ServiceCollection();

    services.AddSmartCities(
      SmartCitiesPersistenceOptions.Create(
        DbProvider.PostgreSql,
        ConnectionStringFor(DbProvider.PostgreSql)));

    using var serviceProvider =
      services.BuildServiceProvider();

    var kernel = serviceProvider
      .GetRequiredService<ICriterionKernel>();

    Assert.IsType<MockCriterionKernel>(kernel);
  }

  [Fact]
  public void Persistence_options_reject_an_unsupported_provider()
  {
    Assert.Throws<ArgumentOutOfRangeException>(
      () => SmartCitiesPersistenceOptions.Create(
        (DbProvider)999,
        "Server=localhost"));
  }

  [Fact]
  public void Persistence_options_reject_a_missing_connection_string()
  {
    Assert.Throws<ArgumentException>(
      () => SmartCitiesPersistenceOptions.Create(
        DbProvider.SqlServer,
        " "));
  }

  private static string ConnectionStringFor(DbProvider provider) =>
    provider switch
    {
      DbProvider.SqlServer =>
        "Server=localhost;Database=SmartCitiesContract;User Id=sa;Password=NotARealPassword1!;TrustServerCertificate=True",
      DbProvider.PostgreSql =>
        "Host=localhost;Database=smartcities_contract;Username=postgres;Password=NotARealPassword1!",
      _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null),
    };
}
