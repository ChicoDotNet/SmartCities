namespace SmartCities.Infrastructure.Persistence;

/// <summary>
/// Identifies the supported relational persistence providers without leaking provider-specific EF Core types.
/// </summary>
public enum DbProvider
{
  /// <summary>Microsoft SQL Server.</summary>
  SqlServer = 0,

  /// <summary>PostgreSQL.</summary>
  PostgreSql = 1,
}
