using SmartCities.Application.Administration;
using SmartCities.Application.Configuration;
using SmartCities.Configuration.Sqlite;
using Xunit;

namespace SmartCities.Configuration.Sqlite.Tests;

public sealed class SqliteControlPlaneAuditStoreTests
{
  [Fact]
  public async Task Audit_entries_are_append_only_ordered_and_partitioned_by_Town_Hall()
  {
    var path = Path.Combine(
      Path.GetTempPath(),
      $"smartcities-audit-{Guid.NewGuid():N}.db");
    var connectionString = $"Data Source={path}";

    try
    {
      using var store =
        new SqliteAdministrationControlPlaneAuditStore(
          connectionString);

      await store.AppendAsync(
        Entry(
          "audit-1",
          "town-hall-a",
          new DateTimeOffset(
            2026, 9, 24, 9, 30, 0, TimeSpan.Zero)),
        TestContext.Current.CancellationToken);
      await store.AppendAsync(
        Entry(
          "audit-2",
          "town-hall-a",
          new DateTimeOffset(
            2026, 9, 24, 9, 31, 0, TimeSpan.Zero)),
        TestContext.Current.CancellationToken);
      await store.AppendAsync(
        Entry(
          "audit-3",
          "town-hall-b",
          new DateTimeOffset(
            2026, 9, 24, 9, 32, 0, TimeSpan.Zero)),
        TestContext.Current.CancellationToken);

      var recent = await store.GetRecentAsync(
        "town-hall-a",
        100,
        TestContext.Current.CancellationToken);

      Assert.Equal(
        ["audit-2", "audit-1"],
        recent.Select(static item => item.EventId));
    }
    finally
    {
      File.Delete(path);
    }
  }

  [Fact]
  public async Task Configuration_mutation_and_audit_entry_commit_together()
  {
    var path = Path.Combine(
      Path.GetTempPath(),
      $"smartcities-audit-mutation-{Guid.NewGuid():N}.db");
    var connectionString = $"Data Source={path}";

    try
    {
      using var settings =
        new SqliteNonSensitiveConfigurationStore(
          connectionString);
      using var audit =
        new SqliteAdministrationControlPlaneAuditStore(
          connectionString);
      var entry = Entry(
        "audit-feature",
        "town-hall-a",
        DateTimeOffset.UtcNow);

      await settings.SetAsync(
        "town-hall-a",
        "feature:citizen-mobility:enabled",
        "false",
        entry,
        TestContext.Current.CancellationToken);

      Assert.Equal(
        "false",
        await settings.GetAsync(
          "town-hall-a",
          "feature:citizen-mobility:enabled",
          TestContext.Current.CancellationToken));

      var persisted = await audit.GetRecentAsync(
        "town-hall-a",
        10,
        TestContext.Current.CancellationToken);

      Assert.Equal(
        "audit-feature",
        Assert.Single(persisted).EventId);
    }
    finally
    {
      File.Delete(path);
    }
  }

  private static AdministrationControlPlaneAuditEvent Entry(
    string eventId,
    string townHallId,
    DateTimeOffset occurredAtUtc) =>
    AdministrationControlPlaneAuditEvent.Create(
      eventId,
      townHallId,
      occurredAtUtc,
      "provider:tenant:official",
      "provider",
      AdministrationAuditActions.FeatureFlagSet,
      AdministrationAuditResourceTypes.FeatureFlag,
      "citizen-mobility",
      "citizen-mobility",
      "true",
      "false",
      "corr-001");
}
