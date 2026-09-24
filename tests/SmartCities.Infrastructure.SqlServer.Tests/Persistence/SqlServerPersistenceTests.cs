using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartCities.Infrastructure.Persistence;
using SmartCities.Infrastructure.SqlServer.Persistence;
using Xunit;

namespace SmartCities.Infrastructure.SqlServer.Tests.Persistence;

public sealed class SqlServerPersistenceTests
{
  [Fact]
  public void Db_provider_contract_names_both_supported_relational_providers()
  {
    Assert.Equal(
      [DbProvider.SqlServer, DbProvider.PostgreSql],
      Enum.GetValues<DbProvider>());
  }

  [Fact]
  public void Sql_server_configuration_uses_its_own_migrations_assembly()
  {
    var builder = new DbContextOptionsBuilder<SmartCitiesDbContext>();

    SqlServerPersistence.Configure(
      builder,
      "Server=localhost;Database=SmartCitiesContract;User Id=sa;Password=NotARealPassword1!;TrustServerCertificate=True");

    using var context = new SmartCitiesDbContext(builder.Options);

    Assert.Equal(
      "Microsoft.EntityFrameworkCore.SqlServer",
      context.Database.ProviderName);
    Assert.Equal(
      "SmartCities.Infrastructure.SqlServer",
      context.GetService<IMigrationsAssembly>().Assembly.GetName().Name);
  }

  [Fact]
  public void Sql_server_migration_assembly_contains_the_initial_persistence_migration()
  {
    var builder = new DbContextOptionsBuilder<SmartCitiesDbContext>();

    SqlServerPersistence.Configure(
      builder,
      "Server=localhost;Database=SmartCitiesContract;User Id=sa;Password=NotARealPassword1!;TrustServerCertificate=True");

    using var context = new SmartCitiesDbContext(builder.Options);

    Assert.Contains(
      "20260923120000_InitialCitizenMobilityPersistence",
      context.Database.GetMigrations());
    Assert.Contains(
      "20260923180500_AddDecisionReviewPersistence",
      context.Database.GetMigrations());
  }

  [Fact]
  public void Initial_sql_server_migration_generates_the_expected_relational_shape()
  {
    var builder = new DbContextOptionsBuilder<SmartCitiesDbContext>();

    SqlServerPersistence.Configure(
      builder,
      "Server=localhost;Database=SmartCitiesContract;User Id=sa;Password=NotARealPassword1!;TrustServerCertificate=True");

    using var context = new SmartCitiesDbContext(builder.Options);
    var script = context.GetService<IMigrator>().GenerateScript();

    Assert.Contains("CitizenMobilityReports", script, StringComparison.Ordinal);
    Assert.Contains("CitizenMobilityEvidenceReferences", script, StringComparison.Ordinal);
    Assert.Contains("ReportId", script, StringComparison.Ordinal);
    Assert.Contains("CaseId", script, StringComparison.Ordinal);
    Assert.Contains("DecisionReviews", script, StringComparison.Ordinal);
    Assert.Contains("AuthoritySubjectId", script, StringComparison.Ordinal);
  }
}
