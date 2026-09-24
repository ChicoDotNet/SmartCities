using SmartCities.Configuration.Sqlite;
using Xunit;

namespace SmartCities.Configuration.Sqlite.Tests;

public sealed class SqliteNonSensitiveConfigurationStoreTests
{
  [Fact]
  public async Task Values_persist_across_store_instances_and_remain_isolated_by_town_hall()
  {
    var databasePath = Path.Combine(
      Path.GetTempPath(),
      $"smartcities-config-{Guid.NewGuid():N}.db");
    var connectionString =
      $"Data Source={databasePath}";

    try
    {
      var first = new SqliteNonSensitiveConfigurationStore(
        connectionString);

      await first.SetAsync(
        "town-hall-a",
        "feature:citizen-mobility:enabled",
        "false",
        TestContext.Current.CancellationToken);

      var second =
        new SqliteNonSensitiveConfigurationStore(
          connectionString);

      var persisted = await second.GetAsync(
        "town-hall-a",
        "feature:citizen-mobility:enabled",
        TestContext.Current.CancellationToken);
      var otherTownHall = await second.GetAsync(
        "town-hall-b",
        "feature:citizen-mobility:enabled",
        TestContext.Current.CancellationToken);

      Assert.Equal("false", persisted);
      Assert.Null(otherTownHall);
    }
    finally
    {
      File.Delete(databasePath);
    }
  }

  [Fact]
  public async Task Store_round_trips_non_sensitive_text_without_sql_interpolation()
  {
    var databasePath = Path.Combine(
      Path.GetTempPath(),
      $"smartcities-config-{Guid.NewGuid():N}.db");
    var connectionString =
      $"Data Source={databasePath}";

    try
    {
      var store = new SqliteNonSensitiveConfigurationStore(
        connectionString);
      const string value =
        "O'Hara deployment label";

      await store.SetAsync(
        "town-hall-'quoted",
        "ui:label",
        value,
        TestContext.Current.CancellationToken);

      var persisted = await store.GetAsync(
        "town-hall-'quoted",
        "ui:label",
        TestContext.Current.CancellationToken);

      Assert.Equal(value, persisted);
    }
    finally
    {
      File.Delete(databasePath);
    }
  }
}
