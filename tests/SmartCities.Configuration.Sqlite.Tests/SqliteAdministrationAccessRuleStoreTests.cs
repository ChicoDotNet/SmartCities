using SmartCities.Application.Administration;
using SmartCities.Configuration.Sqlite;
using Xunit;

namespace SmartCities.Configuration.Sqlite.Tests;

public sealed class SqliteAdministrationAccessRuleStoreTests
{
  [Fact]
  public async Task Rules_persist_across_store_instances_and_are_partitioned_by_Town_Hall()
  {
    var databasePath = Path.Combine(
      Path.GetTempPath(),
      $"smartcities-admin-{Guid.NewGuid():N}.db");
    var connectionString =
      $"Data Source={databasePath}";

    try
    {
      using (var first =
        new SqliteAdministrationAccessRuleStore(
          connectionString))
      {
        await first.AddAsync(
          "town-hall-a",
          AdministrationAccessRule.Create(
            "rule-001",
            AdministrationAccessRuleKind.EmailDomain,
            "townhall.gov"),
          TestContext.Current.CancellationToken);
      }

      using var second =
        new SqliteAdministrationAccessRuleStore(
          connectionString);

      var firstTownHall = await second.GetAllAsync(
        "town-hall-a",
        TestContext.Current.CancellationToken);
      var secondTownHall = await second.GetAllAsync(
        "town-hall-b",
        TestContext.Current.CancellationToken);

      var rule = Assert.Single(firstTownHall);
      Assert.Equal(
        AdministrationAccessRuleKind.EmailDomain,
        rule.Kind);
      Assert.Equal(
        "townhall.gov",
        rule.Value);
      Assert.Empty(secondTownHall);
    }
    finally
    {
      File.Delete(databasePath);
    }
  }

  [Fact]
  public async Task Deleting_a_rule_does_not_affect_other_Town_Halls()
  {
    var databasePath = Path.Combine(
      Path.GetTempPath(),
      $"smartcities-admin-{Guid.NewGuid():N}.db");
    var connectionString =
      $"Data Source={databasePath}";

    try
    {
      using var store =
        new SqliteAdministrationAccessRuleStore(
          connectionString);

      await store.AddAsync(
        "town-hall-a",
        AdministrationAccessRule.Create(
          "shared-id",
          AdministrationAccessRuleKind.Email,
          "a@example.com"),
        TestContext.Current.CancellationToken);
      await store.AddAsync(
        "town-hall-b",
        AdministrationAccessRule.Create(
          "shared-id",
          AdministrationAccessRuleKind.Email,
          "b@example.com"),
        TestContext.Current.CancellationToken);

      Assert.True(
        await store.DeleteAsync(
          "town-hall-a",
          "shared-id",
          TestContext.Current.CancellationToken));

      Assert.Empty(
        await store.GetAllAsync(
          "town-hall-a",
          TestContext.Current.CancellationToken));
      Assert.Single(
        await store.GetAllAsync(
          "town-hall-b",
          TestContext.Current.CancellationToken));
    }
    finally
    {
      File.Delete(databasePath);
    }
  }
}
