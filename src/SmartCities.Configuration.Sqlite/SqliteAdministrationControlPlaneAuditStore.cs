using System.Globalization;
using Microsoft.Data.Sqlite;
using SmartCities.Application.Administration;

namespace SmartCities.Configuration.Sqlite;

/// <summary>
/// Persists immutable Town Hall control-plane audit events in SQLite.
/// </summary>
public sealed class SqliteAdministrationControlPlaneAuditStore
  : IAdministrationControlPlaneAuditStore,
    IDisposable
{
  private const int MaximumTownHallIdLength = 128;
  private const int MaximumLimit = 200;

  private readonly string connectionString;
  private readonly SemaphoreSlim schemaGate = new(1, 1);
  private bool schemaReady;

  /// <summary>Initializes the append-only audit store.</summary>
  public SqliteAdministrationControlPlaneAuditStore(
    string connectionString)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
    _ = new SqliteConnectionStringBuilder(connectionString);
    this.connectionString = connectionString;
  }

  /// <inheritdoc />
  public async Task AppendAsync(
    AdministrationControlPlaneAuditEvent auditEvent,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(auditEvent);

    await using var connection =
      new SqliteConnection(connectionString);
    await connection
      .OpenAsync(cancellationToken)
      .ConfigureAwait(false);
    await EnsureSchemaAsync(
        connection,
        cancellationToken)
      .ConfigureAwait(false);

    await SqliteControlPlaneAuditPersistence
      .AppendAsync(
        connection,
        transaction: null,
        auditEvent,
        cancellationToken)
      .ConfigureAwait(false);
  }

  /// <inheritdoc />
  public async Task<IReadOnlyList<AdministrationControlPlaneAuditEvent>>
    GetRecentAsync(
      string townHallId,
      int limit,
      CancellationToken cancellationToken = default)
  {
    ValidateTownHallId(townHallId);

    if (limit is < 1 or > MaximumLimit)
    {
      throw new ArgumentOutOfRangeException(
        nameof(limit),
        limit,
        $"Audit history limit must be between 1 and {MaximumLimit}.");
    }

    await using var connection =
      new SqliteConnection(connectionString);
    await connection
      .OpenAsync(cancellationToken)
      .ConfigureAwait(false);
    await EnsureSchemaAsync(
        connection,
        cancellationToken)
      .ConfigureAwait(false);

    await using var command = connection.CreateCommand();
    command.CommandText =
      """
      SELECT
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
        correlation_id
      FROM administration_control_plane_audit
      WHERE town_hall_id = $townHallId
      ORDER BY occurred_at_utc DESC, event_id DESC
      LIMIT $limit;
      """;
    command.Parameters.Add("$townHallId", SqliteType.Text)
      .Value = townHallId;
    command.Parameters.Add("$limit", SqliteType.Integer)
      .Value = limit;

    var events =
      new List<AdministrationControlPlaneAuditEvent>();

    await using var reader = await command
      .ExecuteReaderAsync(cancellationToken)
      .ConfigureAwait(false);

    while (await reader
      .ReadAsync(cancellationToken)
      .ConfigureAwait(false))
    {
      events.Add(
        AdministrationControlPlaneAuditEvent.Create(
          reader.GetString(0),
          reader.GetString(1),
          DateTimeOffset.Parse(
            reader.GetString(2),
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind),
          reader.GetString(3),
          reader.GetString(4),
          reader.GetString(5),
          reader.GetString(6),
          reader.GetString(7),
          reader.GetString(8),
          reader.IsDBNull(9) ? null : reader.GetString(9),
          reader.IsDBNull(10) ? null : reader.GetString(10),
          reader.GetString(11)));
    }

    return events;
  }

  /// <inheritdoc />
  public void Dispose()
  {
    schemaGate.Dispose();
  }

  private async Task EnsureSchemaAsync(
    SqliteConnection connection,
    CancellationToken cancellationToken)
  {
    if (schemaReady)
    {
      return;
    }

    await schemaGate
      .WaitAsync(cancellationToken)
      .ConfigureAwait(false);

    try
    {
      if (schemaReady)
      {
        return;
      }

      await SqliteControlPlaneAuditPersistence
        .EnsureSchemaAsync(
          connection,
          cancellationToken)
        .ConfigureAwait(false);

      schemaReady = true;
    }
    finally
    {
      schemaGate.Release();
    }
  }

  private static void ValidateTownHallId(
    string townHallId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(townHallId);

    if (townHallId.Length > MaximumTownHallIdLength)
    {
      throw new ArgumentException(
        $"Town Hall identifiers cannot exceed {MaximumTownHallIdLength} characters.",
        nameof(townHallId));
    }
  }
}
