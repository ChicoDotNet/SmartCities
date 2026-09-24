using System.Globalization;
using Microsoft.Data.Sqlite;
using SmartCities.Application.Administration;

namespace SmartCities.Configuration.Sqlite;

internal static class SqliteControlPlaneAuditPersistence
{
  internal static async Task EnsureSchemaAsync(
    SqliteConnection connection,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(connection);

    await using var command = connection.CreateCommand();
    command.CommandText =
      """
      CREATE TABLE IF NOT EXISTS administration_control_plane_audit (
        event_id TEXT NOT NULL PRIMARY KEY,
        town_hall_id TEXT NOT NULL,
        occurred_at_utc TEXT NOT NULL,
        actor_subject_id TEXT NOT NULL,
        actor_identity_provider TEXT NOT NULL,
        action TEXT NOT NULL,
        resource_type TEXT NOT NULL,
        resource_id TEXT NOT NULL,
        descriptor TEXT NOT NULL,
        previous_value TEXT NULL,
        new_value TEXT NULL,
        correlation_id TEXT NOT NULL
      );

      CREATE INDEX IF NOT EXISTS ix_administration_control_plane_audit_town_hall_time
      ON administration_control_plane_audit (
        town_hall_id,
        occurred_at_utc DESC,
        event_id DESC
      );
      """;

    await command
      .ExecuteNonQueryAsync(cancellationToken)
      .ConfigureAwait(false);
  }

  internal static async Task AppendAsync(
    SqliteConnection connection,
    SqliteTransaction? transaction,
    AdministrationControlPlaneAuditEvent auditEvent,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(connection);
    ArgumentNullException.ThrowIfNull(auditEvent);

    await using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText =
      """
      INSERT INTO administration_control_plane_audit (
        event_id,
        town_hall_id,
        occurred_at_utc,
        actor_subject_id,
        actor_identity_provider,
        action,
        resource_type,
        resource_id,
        descriptor,
        previous_value,
        new_value,
        correlation_id)
      VALUES (
        $eventId,
        $townHallId,
        $occurredAtUtc,
        $actorSubjectId,
        $actorIdentityProvider,
        $action,
        $resourceType,
        $resourceId,
        $descriptor,
        $previousValue,
        $newValue,
        $correlationId);
      """;

    command.Parameters.Add("$eventId", SqliteType.Text)
      .Value = auditEvent.EventId;
    command.Parameters.Add("$townHallId", SqliteType.Text)
      .Value = auditEvent.TownHallId;
    command.Parameters.Add("$occurredAtUtc", SqliteType.Text)
      .Value = auditEvent.OccurredAtUtc.ToString(
        "O",
        CultureInfo.InvariantCulture);
    command.Parameters.Add("$actorSubjectId", SqliteType.Text)
      .Value = auditEvent.ActorSubjectId;
    command.Parameters.Add("$actorIdentityProvider", SqliteType.Text)
      .Value = auditEvent.ActorIdentityProvider;
    command.Parameters.Add("$action", SqliteType.Text)
      .Value = auditEvent.Action;
    command.Parameters.Add("$resourceType", SqliteType.Text)
      .Value = auditEvent.ResourceType;
    command.Parameters.Add("$resourceId", SqliteType.Text)
      .Value = auditEvent.ResourceId;
    command.Parameters.Add("$descriptor", SqliteType.Text)
      .Value = auditEvent.Descriptor;
    command.Parameters.Add("$previousValue", SqliteType.Text)
      .Value = (object?)auditEvent.PreviousValue ?? DBNull.Value;
    command.Parameters.Add("$newValue", SqliteType.Text)
      .Value = (object?)auditEvent.NewValue ?? DBNull.Value;
    command.Parameters.Add("$correlationId", SqliteType.Text)
      .Value = auditEvent.CorrelationId;

    await command
      .ExecuteNonQueryAsync(cancellationToken)
      .ConfigureAwait(false);
  }
}
