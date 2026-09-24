using System.Globalization;
using Microsoft.Data.Sqlite;
using SmartCities.Application.Configuration;

namespace SmartCities.Configuration.Sqlite;

/// <summary>
/// Stores explicitly non-sensitive Town Hall configuration in a dedicated SQLite database.
/// </summary>
/// <remarks>
/// Values are persisted as plaintext by design. This adapter must never receive secrets, credentials,
/// authentication material, private keys, tokens, citizen evidence, or other sensitive data.
/// </remarks>
public sealed class SqliteNonSensitiveConfigurationStore
  : INonSensitiveConfigurationStore
{
  private const int MaximumTownHallIdLength = 128;
  private const int MaximumKeyLength = 256;
  private const int MaximumValueLength = 32768;

  private readonly string connectionString;
  private readonly SemaphoreSlim schemaGate =
    new(1, 1);
  private bool schemaReady;

  /// <summary>Initializes the store with a dedicated SQLite connection string.</summary>
  public SqliteNonSensitiveConfigurationStore(
    string connectionString)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      connectionString);

    _ = new SqliteConnectionStringBuilder(
      connectionString);

    this.connectionString =
      connectionString;
  }

  /// <inheritdoc />
  public async Task<string?> GetAsync(
    string townHallId,
    string key,
    CancellationToken cancellationToken = default)
  {
    ValidateTownHallId(townHallId);
    ValidateKey(key);

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
      SELECT setting_value
      FROM non_sensitive_settings
      WHERE town_hall_id = $townHallId
        AND setting_key = $key
      LIMIT 1;
      """;

    command.Parameters
      .Add(
        "$townHallId",
        SqliteType.Text)
      .Value = townHallId;
    command.Parameters
      .Add(
        "$key",
        SqliteType.Text)
      .Value = key;

    var value = await command
      .ExecuteScalarAsync(cancellationToken)
      .ConfigureAwait(false);

    return value is null or DBNull
      ? null
      : Convert.ToString(
          value,
          CultureInfo.InvariantCulture);
  }

  /// <inheritdoc />
  public async Task SetAsync(
    string townHallId,
    string key,
    string value,
    CancellationToken cancellationToken = default)
  {
    ValidateTownHallId(townHallId);
    ValidateKey(key);
    ArgumentNullException.ThrowIfNull(value);

    if (value.Length > MaximumValueLength)
    {
      throw new ArgumentException(
        $"Non-sensitive configuration values cannot exceed {MaximumValueLength} characters.",
        nameof(value));
    }

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
      INSERT INTO non_sensitive_settings (
        town_hall_id,
        setting_key,
        setting_value,
        updated_at_utc)
      VALUES (
        $townHallId,
        $key,
        $value,
        $updatedAtUtc)
      ON CONFLICT(town_hall_id, setting_key)
      DO UPDATE SET
        setting_value = excluded.setting_value,
        updated_at_utc = excluded.updated_at_utc;
      """;

    command.Parameters
      .Add(
        "$townHallId",
        SqliteType.Text)
      .Value = townHallId;
    command.Parameters
      .Add(
        "$key",
        SqliteType.Text)
      .Value = key;
    command.Parameters
      .Add(
        "$value",
        SqliteType.Text)
      .Value = value;
    command.Parameters
      .Add(
        "$updatedAtUtc",
        SqliteType.Text)
      .Value = DateTimeOffset.UtcNow.ToString(
        "O",
        CultureInfo.InvariantCulture);

    await command
      .ExecuteNonQueryAsync(cancellationToken)
      .ConfigureAwait(false);
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
        CREATE TABLE IF NOT EXISTS non_sensitive_settings (
          town_hall_id TEXT NOT NULL,
          setting_key TEXT NOT NULL,
          setting_value TEXT NOT NULL,
          updated_at_utc TEXT NOT NULL,
          PRIMARY KEY (town_hall_id, setting_key)
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

  private static void ValidateKey(
    string key)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(key);

    if (key.Length > MaximumKeyLength)
    {
      throw new ArgumentException(
        $"Configuration keys cannot exceed {MaximumKeyLength} characters.",
        nameof(key));
    }
  }
}
