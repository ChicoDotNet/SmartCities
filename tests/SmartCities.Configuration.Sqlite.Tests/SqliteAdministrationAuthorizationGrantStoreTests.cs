using SmartCities.Application.Administration;
using SmartCities.Configuration.Sqlite;
using Xunit;

namespace SmartCities.Configuration.Sqlite.Tests;

public sealed class SqliteAdministrationAuthorizationGrantStoreTests
{
  [Fact]
  public async Task Grants_persist_across_instances_and_are_partitioned_by_Town_Hall()
  {
    var databasePath = Path.Combine(
      Path.GetTempPath(),
      $"smartcities-grants-{Guid.NewGuid():N}.db");
    var connectionString =
      $"Data Source={databasePath}";

    try
    {
      using (var first =
        new SqliteAdministrationAuthorizationGrantStore(
          connectionString))
      {
        await first.AddAsync(
          "town-hall-a",
          AdministrationAuthorizationGrant.Create(
            "grant-001",
            AdministrationAccessRuleKind.Email,
            "Official@Example.com",
            AdministrationAuthorizationGrantKind.Permission,
            "citizen-mobility.manage"),
          TestContext.Current.CancellationToken);
      }

      using var second =
        new SqliteAdministrationAuthorizationGrantStore(
          connectionString);

      var a = await second.GetAllAsync(
        "town-hall-a",
        TestContext.Current.CancellationToken);
      var b = await second.GetAllAsync(
        "town-hall-b",
        TestContext.Current.CancellationToken);

      var grant = Assert.Single(a);
      Assert.Equal("official@example.com", grant.TargetValue);
      Assert.Equal(
        AdministrationAuthorizationGrantKind.Permission,
        grant.Kind);
      Assert.Equal(
        "citizen-mobility.manage",
        grant.Value);
      Assert.Empty(b);
    }
    finally
    {
      File.Delete(databasePath);
    }
  }
}
