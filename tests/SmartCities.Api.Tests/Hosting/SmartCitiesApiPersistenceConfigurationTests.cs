using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Api.Citizens;
using SmartCities.Api.Hosting;
using SmartCities.Application.Citizens;
using SmartCities.Infrastructure.Persistence;
using Xunit;

namespace SmartCities.Api.Tests.Hosting;

public sealed class SmartCitiesApiPersistenceConfigurationTests
{
  [Theory]
  [InlineData(
    "SqlServer",
    "Server=localhost;Database=SmartCitiesContract;User Id=sa;Password=NotARealPassword1!;TrustServerCertificate=True",
    "Microsoft.EntityFrameworkCore.SqlServer",
    "SmartCities.Infrastructure.SqlServer")]
  [InlineData(
    "PostgreSql",
    "Host=localhost;Database=smartcities_contract;Username=postgres;Password=NotARealPassword1!",
    "Npgsql.EntityFrameworkCore.PostgreSQL",
    "SmartCities.Infrastructure.PostgreSql")]
  public void Host_configuration_selects_the_provider_and_resolves_the_controller_graph(
    string dbProvider,
    string connectionString,
    string expectedProviderName,
    string expectedMigrationsAssembly)
  {
    var configuration = BuildConfiguration(
      dbProvider,
      connectionString);
    var services = new ServiceCollection();

    services.AddSmartCitiesFromConfiguration(configuration);

    using var serviceProvider = services.BuildServiceProvider();
    using var scope = serviceProvider.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<SmartCitiesDbContext>();
    var service = scope.ServiceProvider.GetRequiredService<ICitizenMobilityReportService>();
    var controller = ActivatorUtilities.CreateInstance<CitizenMobilityReportsController>(
      scope.ServiceProvider);

    Assert.Equal(expectedProviderName, context.Database.ProviderName);
    Assert.Equal(
      expectedMigrationsAssembly,
      context.GetService<IMigrationsAssembly>().Assembly.GetName().Name);
    Assert.IsType<CitizenMobilityReportService>(service);
    Assert.NotNull(controller);
  }

  [Theory]
  [InlineData("sqlserver", DbProvider.SqlServer)]
  [InlineData("POSTGRESQL", DbProvider.PostgreSql)]
  public void Host_configuration_parses_provider_names_case_insensitively(
    string configuredProvider,
    DbProvider expectedProvider)
  {
    var configuration = BuildConfiguration(
      configuredProvider,
      ConnectionStringFor(expectedProvider));

    var options = configuration.GetSmartCitiesPersistenceOptions();

    Assert.Equal(expectedProvider, options.Provider);
  }

  [Fact]
  public void Host_configuration_rejects_a_missing_provider()
  {
    var configuration = BuildConfiguration(
      dbProvider: null,
      connectionString: ConnectionStringFor(DbProvider.SqlServer));

    var exception = Assert.Throws<InvalidOperationException>(
      () => configuration.GetSmartCitiesPersistenceOptions());

    Assert.Contains(
      "SmartCities:Persistence:DbProvider",
      exception.Message,
      StringComparison.Ordinal);
  }

  [Fact]
  public void Host_configuration_rejects_an_unsupported_provider()
  {
    var configuration = BuildConfiguration(
      "Oracle",
      "Data Source=localhost");

    var exception = Assert.Throws<InvalidOperationException>(
      () => configuration.GetSmartCitiesPersistenceOptions());

    Assert.Contains(
      "Oracle",
      exception.Message,
      StringComparison.Ordinal);
  }

  [Fact]
  public void Host_configuration_rejects_a_missing_connection_string()
  {
    var configuration = BuildConfiguration(
      "SqlServer",
      connectionString: null);

    var exception = Assert.Throws<InvalidOperationException>(
      () => configuration.GetSmartCitiesPersistenceOptions());

    Assert.Contains(
      "SmartCities:Persistence:ConnectionString",
      exception.Message,
      StringComparison.Ordinal);
  }

  private static IConfiguration BuildConfiguration(
    string? dbProvider,
    string? connectionString)
  {
    var values = new Dictionary<string, string?>();

    if (dbProvider is not null)
    {
      values["SmartCities:Persistence:DbProvider"] = dbProvider;
    }

    if (connectionString is not null)
    {
      values["SmartCities:Persistence:ConnectionString"] = connectionString;
    }

    return new ConfigurationBuilder()
      .AddInMemoryCollection(values)
      .Build();
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
