using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartCities.Infrastructure.Persistence;
using SmartCities.Infrastructure.PostgreSql.Persistence;
using SmartCities.Infrastructure.SqlServer.Persistence;
using Xunit;

namespace SmartCities.Infrastructure.PostgreSql.Tests.Persistence;

public sealed class PostgreSqlPersistenceTests
{
  [Fact]
  public void Postgre_sql_configuration_uses_its_own_migrations_assembly()
  {
    var builder = new DbContextOptionsBuilder<SmartCitiesDbContext>();

    PostgreSqlPersistence.Configure(
      builder,
      "Host=localhost;Database=smartcities_contract;Username=postgres;Password=NotARealPassword1!");

    using var context = new SmartCitiesDbContext(builder.Options);

    Assert.Equal(
      DbProvider.PostgreSql,
      PostgreSqlPersistence.Provider);
    Assert.Equal(
      "Npgsql.EntityFrameworkCore.PostgreSQL",
      context.Database.ProviderName);
    Assert.Equal(
      "SmartCities.Infrastructure.PostgreSql",
      context.GetService<IMigrationsAssembly>().Assembly.GetName().Name);
  }

  [Fact]
  public void Postgre_sql_migration_assembly_contains_the_initial_persistence_migration()
  {
    var builder = new DbContextOptionsBuilder<SmartCitiesDbContext>();

    PostgreSqlPersistence.Configure(
      builder,
      "Host=localhost;Database=smartcities_contract;Username=postgres;Password=NotARealPassword1!");

    using var context = new SmartCitiesDbContext(builder.Options);

    Assert.Contains(
      "20260923121000_InitialCitizenMobilityPersistence",
      context.Database.GetMigrations());
    Assert.Contains(
      "20260923180500_AddDecisionReviewPersistence",
      context.Database.GetMigrations());
  }

  [Fact]
  public void Initial_postgre_sql_migration_generates_the_expected_relational_shape()
  {
    var builder = new DbContextOptionsBuilder<SmartCitiesDbContext>();

    PostgreSqlPersistence.Configure(
      builder,
      "Host=localhost;Database=smartcities_contract;Username=postgres;Password=NotARealPassword1!");

    using var context = new SmartCitiesDbContext(builder.Options);
    var script = context.GetService<IMigrator>().GenerateScript();

    Assert.Contains("CitizenMobilityReports", script, StringComparison.Ordinal);
    Assert.Contains("CitizenMobilityEvidenceReferences", script, StringComparison.Ordinal);
    Assert.Contains("ReportId", script, StringComparison.Ordinal);
    Assert.Contains("CaseId", script, StringComparison.Ordinal);
    Assert.Contains("DecisionReviews", script, StringComparison.Ordinal);
    Assert.Contains("AuthoritySubjectId", script, StringComparison.Ordinal);
    Assert.Contains("timestamp with time zone", script, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Production_providers_keep_independent_migration_assemblies()
  {
    var sqlServerBuilder = new DbContextOptionsBuilder<SmartCitiesDbContext>();
    SqlServerPersistence.Configure(
      sqlServerBuilder,
      "Server=localhost;Database=SmartCitiesContract;User Id=sa;Password=NotARealPassword1!;TrustServerCertificate=True");

    var postgreSqlBuilder = new DbContextOptionsBuilder<SmartCitiesDbContext>();
    PostgreSqlPersistence.Configure(
      postgreSqlBuilder,
      "Host=localhost;Database=smartcities_contract;Username=postgres;Password=NotARealPassword1!");

    using var sqlServer = new SmartCitiesDbContext(sqlServerBuilder.Options);
    using var postgreSql = new SmartCitiesDbContext(postgreSqlBuilder.Options);

    var sqlServerAssembly = sqlServer.GetService<IMigrationsAssembly>().Assembly.GetName().Name;
    var postgreSqlAssembly = postgreSql.GetService<IMigrationsAssembly>().Assembly.GetName().Name;

    Assert.Equal("SmartCities.Infrastructure.SqlServer", sqlServerAssembly);
    Assert.Equal("SmartCities.Infrastructure.PostgreSql", postgreSqlAssembly);
    Assert.NotEqual(sqlServerAssembly, postgreSqlAssembly);
  }
}
