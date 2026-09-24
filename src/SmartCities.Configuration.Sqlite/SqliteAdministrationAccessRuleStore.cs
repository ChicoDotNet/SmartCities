using System.Globalization;
using Microsoft.Data.Sqlite;
using SmartCities.Application.Administration;

namespace SmartCities.Configuration.Sqlite;

/// <summary>
/// Persists Town Hall Administration admission rules in the local SQLite control-plane database.
/// </summary>
/// <remarks>
/// Rule values can contain staff email addresses and therefore are access-control/PII metadata, not generic
/// non-sensitive settings. Passwords, tokens, keys, citizen evidence, and other secrets remain forbidden.
/// </remarks>
public sealed class SqliteAdministrationAccessRuleStore
  : IAdministrationAccessRuleStore,
    IDisposable
{
  private const int MaximumTownHallIdLength = 128;

  private readonly string connectionString;
  private readonly SemaphoreSlim schemaGate =
    new(1, 1);
  private bool schemaReady;

  /// <summary>Initializes the whitelist store with the shared control-plane SQLite connection string.</summary>
  public SqliteAdministrationAccessRuleStore(
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
  public async Task<IReadOnlyList<AdministrationAccessRule>>
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
      SELECT rule_id, rule_kind, rule_value
      FROM administration_access_rules
      WHERE town_hall_id = $townHallId
      ORDER BY rule_kind, rule_value, rule_id;
      """;
    command.Parameters
      .Add("$townHallId", SqliteType.Text)
      .Value = townHallId;

    var rules =
      new List<AdministrationAccessRule>();

    await using var reader = await command
      .ExecuteReaderAsync(cancellationToken)
      .ConfigureAwait(false);

    while (await reader
      .ReadAsync(cancellationToken)
      .ConfigureAwait(false))
    {
      rules.Add(
        AdministrationAccessRule.Create(
          reader.GetString(0),
          checked(
            (AdministrationAccessRuleKind)
              reader.GetInt32(1)),
          reader.GetString(2)));
    }

    return rules;
  }
  /// <inheritdoc />
  public Task AddAsync(
    string townHallId,
    AdministrationAccessRule rule,
    CancellationToken cancellationToken = default) =>
    AddCoreAsync(
      townHallId,
      rule,
      auditEvent: null,
      cancellationToken);

  /// <inheritdoc />
  public Task AddAsync(
    string townHallId,
    AdministrationAccessRule rule,
    AdministrationControlPlaneAuditEvent auditEvent,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(auditEvent);
    return AddCoreAsync(
      townHallId,
      rule,
      auditEvent,
      cancellationToken);
  }

  private async Task AddCoreAsync(
    string townHallId,
    AdministrationAccessRule rule,
    AdministrationControlPlaneAuditEvent? auditEvent,
    CancellationToken cancellationToken)
  {
    ValidateTownHallId(townHallId);
    ArgumentNullException.ThrowIfNull(rule);
    ValidateAuditPartition(
      townHallId,
      auditEvent);
    await EnsureSchemaAsync(cancellationToken)
      .ConfigureAwait(false);

    await using var connection =
      new SqliteConnection(connectionString);
    await connection
      .OpenAsync(cancellationToken)
      .ConfigureAwait(false);

    if (auditEvent is not null)
    {
      await SqliteControlPlaneAuditPersistence
        .EnsureSchemaAsync(
          connection,
          cancellationToken)
        .ConfigureAwait(false);
    }

    await using var transaction =
      (SqliteTransaction)await connection
        .BeginTransactionAsync(cancellationToken)
        .ConfigureAwait(false);

    await using var command =
      connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText =
      """
      INSERT INTO administration_access_rules (
        town_hall_id,
        rule_id,
        rule_kind,
        rule_value,
        created_at_utc)
      VALUES (
        $townHallId,
        $ruleId,
        $ruleKind,
        $ruleValue,
        $createdAtUtc);
      """;
    command.Parameters
      .Add("$townHallId", SqliteType.Text)
      .Value = townHallId;
    command.Parameters
      .Add("$ruleId", SqliteType.Text)
      .Value = rule.RuleId;
    command.Parameters
      .Add("$ruleKind", SqliteType.Integer)
      .Value = (int)rule.Kind;
    command.Parameters
      .Add("$ruleValue", SqliteType.Text)
      .Value = rule.Value;
    command.Parameters
      .Add("$createdAtUtc", SqliteType.Text)
      .Value = DateTimeOffset.UtcNow.ToString(
        "O",
        CultureInfo.InvariantCulture);

    await command
      .ExecuteNonQueryAsync(cancellationToken)
      .ConfigureAwait(false);

    if (auditEvent is not null)
    {
      await SqliteControlPlaneAuditPersistence
        .AppendAsync(
          connection,
          transaction,
          auditEvent,
          cancellationToken)
        .ConfigureAwait(false);
    }

    await transaction
      .CommitAsync(cancellationToken)
      .ConfigureAwait(false);
  }

  /// <inheritdoc />
  public Task<bool> DeleteAsync(
    string townHallId,
    string ruleId,
    CancellationToken cancellationToken = default) =>
    DeleteCoreAsync(
      townHallId,
      ruleId,
      auditEvent: null,
      cancellationToken);

  /// <inheritdoc />
  public Task<bool> DeleteAsync(
    string townHallId,
    string ruleId,
    AdministrationControlPlaneAuditEvent auditEvent,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(auditEvent);
    return DeleteCoreAsync(
      townHallId,
      ruleId,
      auditEvent,
      cancellationToken);
  }

  private async Task<bool> DeleteCoreAsync(
    string townHallId,
    string ruleId,
    AdministrationControlPlaneAuditEvent? auditEvent,
    CancellationToken cancellationToken)
  {
    ValidateTownHallId(townHallId);
    ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
    ValidateAuditPartition(
      townHallId,
      auditEvent);
    await EnsureSchemaAsync(cancellationToken)
      .ConfigureAwait(false);

    await using var connection =
      new SqliteConnection(connectionString);
    await connection
      .OpenAsync(cancellationToken)
      .ConfigureAwait(false);

    if (auditEvent is not null)
    {
      await SqliteControlPlaneAuditPersistence
        .EnsureSchemaAsync(
          connection,
          cancellationToken)
        .ConfigureAwait(false);
    }

    await using var transaction =
      (SqliteTransaction)await connection
        .BeginTransactionAsync(cancellationToken)
        .ConfigureAwait(false);

    await using var command =
      connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText =
      """
      DELETE FROM administration_access_rules
      WHERE town_hall_id = $townHallId
        AND rule_id = $ruleId;
      """;
    command.Parameters
      .Add("$townHallId", SqliteType.Text)
      .Value = townHallId;
    command.Parameters
      .Add("$ruleId", SqliteType.Text)
      .Value = ruleId.Trim();

    var deleted = await command
      .ExecuteNonQueryAsync(cancellationToken)
      .ConfigureAwait(false) == 1;

    if (deleted && auditEvent is not null)
    {
      await SqliteControlPlaneAuditPersistence
        .AppendAsync(
          connection,
          transaction,
          auditEvent,
          cancellationToken)
        .ConfigureAwait(false);
    }

    await transaction
      .CommitAsync(cancellationToken)
      .ConfigureAwait(false);

    return deleted;
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
        CREATE TABLE IF NOT EXISTS administration_access_rules (
          town_hall_id TEXT NOT NULL,
          rule_id TEXT NOT NULL,
          rule_kind INTEGER NOT NULL,
          rule_value TEXT NOT NULL,
          created_at_utc TEXT NOT NULL,
          PRIMARY KEY (town_hall_id, rule_id),
          UNIQUE (town_hall_id, rule_kind, rule_value)
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

  private static void ValidateAuditPartition(
    string townHallId,
    AdministrationControlPlaneAuditEvent? auditEvent)
  {
    if (auditEvent is not null
      && !string.Equals(
        auditEvent.TownHallId,
        townHallId,
        StringComparison.Ordinal))
    {
      throw new ArgumentException(
        "Audit event Town Hall must match the mutation partition.",
        nameof(auditEvent));
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
