using System.Globalization;
using Microsoft.Data.Sqlite;
using SmartCities.Application.Administration;

namespace SmartCities.Configuration.Sqlite;

/// <summary>
/// Persists typed Administration authorization grants in the local per-Town-Hall SQLite control plane.
/// </summary>
public sealed class SqliteAdministrationAuthorizationGrantStore
  : IAdministrationAuthorizationGrantStore,
    IDisposable
{
  private const int MaximumTownHallIdLength = 128;

  private readonly string connectionString;
  private readonly SemaphoreSlim schemaGate =
    new(1, 1);
  private bool schemaReady;

  /// <summary>Initializes the grant store with the shared control-plane SQLite connection string.</summary>
  public SqliteAdministrationAuthorizationGrantStore(
    string connectionString)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      connectionString);

    _ = new SqliteConnectionStringBuilder(
      connectionString);

    this.connectionString = connectionString;
  }

  /// <inheritdoc />
  public async Task<IReadOnlyList<AdministrationAuthorizationGrant>>
    GetAllAsync(
      string townHallId,
      CancellationToken cancellationToken = default)
  {
    ValidateTownHallId(townHallId);
    await EnsureSchemaAsync(cancellationToken)
      .ConfigureAwait(false);

    await using var connection =
      new SqliteConnection(connectionString);
    await connection
      .OpenAsync(cancellationToken)
      .ConfigureAwait(false);

    await using var command =
      connection.CreateCommand();
    command.CommandText =
      """
      SELECT
        grant_id,
        target_kind,
        target_value,
        grant_kind,
        grant_value
      FROM administration_authorization_grants
      WHERE town_hall_id = $townHallId
      ORDER BY target_kind, target_value, grant_kind, grant_value, grant_id;
      """;
    command.Parameters
      .Add("$townHallId", SqliteType.Text)
      .Value = townHallId;

    var grants =
      new List<AdministrationAuthorizationGrant>();

    await using var reader = await command
      .ExecuteReaderAsync(cancellationToken)
      .ConfigureAwait(false);

    while (await reader
      .ReadAsync(cancellationToken)
      .ConfigureAwait(false))
    {
      grants.Add(
        AdministrationAuthorizationGrant.Create(
          reader.GetString(0),
          checked(
            (AdministrationAccessRuleKind)
              reader.GetInt32(1)),
          reader.GetString(2),
          checked(
            (AdministrationAuthorizationGrantKind)
              reader.GetInt32(3)),
          reader.GetString(4)));
    }

    return grants;
  }

  /// <inheritdoc />
  public async Task AddAsync(
    string townHallId,
    AdministrationAuthorizationGrant grant,
    CancellationToken cancellationToken = default)
  {
    ValidateTownHallId(townHallId);
    ArgumentNullException.ThrowIfNull(grant);
    await EnsureSchemaAsync(cancellationToken)
      .ConfigureAwait(false);

    await using var connection =
      new SqliteConnection(connectionString);
    await connection
      .OpenAsync(cancellationToken)
      .ConfigureAwait(false);

    await using var command =
      connection.CreateCommand();
    command.CommandText =
      """
      INSERT INTO administration_authorization_grants (
        town_hall_id,
        grant_id,
        target_kind,
        target_value,
        grant_kind,
        grant_value,
        created_at_utc)
      VALUES (
        $townHallId,
        $grantId,
        $targetKind,
        $targetValue,
        $grantKind,
        $grantValue,
        $createdAtUtc);
      """;
    command.Parameters
      .Add("$townHallId", SqliteType.Text)
      .Value = townHallId;
    command.Parameters
      .Add("$grantId", SqliteType.Text)
      .Value = grant.GrantId;
    command.Parameters
      .Add("$targetKind", SqliteType.Integer)
      .Value = (int)grant.TargetKind;
    command.Parameters
      .Add("$targetValue", SqliteType.Text)
      .Value = grant.TargetValue;
    command.Parameters
      .Add("$grantKind", SqliteType.Integer)
      .Value = (int)grant.Kind;
    command.Parameters
      .Add("$grantValue", SqliteType.Text)
      .Value = grant.Value;
    command.Parameters
      .Add("$createdAtUtc", SqliteType.Text)
      .Value = DateTimeOffset.UtcNow.ToString(
        "O",
        CultureInfo.InvariantCulture);

    await command
      .ExecuteNonQueryAsync(cancellationToken)
      .ConfigureAwait(false);
  }

  /// <inheritdoc />
  public async Task<bool> DeleteAsync(
    string townHallId,
    string grantId,
    CancellationToken cancellationToken = default)
  {
    ValidateTownHallId(townHallId);
    ArgumentException.ThrowIfNullOrWhiteSpace(grantId);
    await EnsureSchemaAsync(cancellationToken)
      .ConfigureAwait(false);

    await using var connection =
      new SqliteConnection(connectionString);
    await connection
      .OpenAsync(cancellationToken)
      .ConfigureAwait(false);

    await using var command =
      connection.CreateCommand();
    command.CommandText =
      """
      DELETE FROM administration_authorization_grants
      WHERE town_hall_id = $townHallId
        AND grant_id = $grantId;
      """;
    command.Parameters
      .Add("$townHallId", SqliteType.Text)
      .Value = townHallId;
    command.Parameters
      .Add("$grantId", SqliteType.Text)
      .Value = grantId.Trim();

    return await command
      .ExecuteNonQueryAsync(cancellationToken)
      .ConfigureAwait(false) == 1;
  }

  /// <inheritdoc />
  public void Dispose()
  {
    schemaGate.Dispose();
  }

  private async Task EnsureSchemaAsync(
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

      await using var connection =
        new SqliteConnection(connectionString);
      await connection
        .OpenAsync(cancellationToken)
        .ConfigureAwait(false);

      await using var command =
        connection.CreateCommand();
      command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS administration_authorization_grants (
          town_hall_id TEXT NOT NULL,
          grant_id TEXT NOT NULL,
          target_kind INTEGER NOT NULL,
          target_value TEXT NOT NULL,
          grant_kind INTEGER NOT NULL,
          grant_value TEXT NOT NULL,
          created_at_utc TEXT NOT NULL,
          PRIMARY KEY (town_hall_id, grant_id),
          UNIQUE (
            town_hall_id,
            target_kind,
            target_value,
            grant_kind,
            grant_value)
        );
        """;

      await command
        .ExecuteNonQueryAsync(cancellationToken)
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
    ArgumentException.ThrowIfNullOrWhiteSpace(
      townHallId);

    if (townHallId.Length > MaximumTownHallIdLength)
    {
      throw new ArgumentException(
        $"Town Hall identifiers cannot exceed {MaximumTownHallIdLength} characters.",
        nameof(townHallId));
    }
  }
}
