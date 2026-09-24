namespace SmartCities.Composition;

/// <summary>
/// Validated host configuration for per-Town-Hall non-sensitive feature management.
/// </summary>
public sealed record SmartCitiesFeatureManagementOptions
{
  private SmartCitiesFeatureManagementOptions(
    string townHallId,
    string sqliteConnectionString)
  {
    TownHallId = townHallId;
    SqliteConnectionString = sqliteConnectionString;
  }

  /// <summary>Gets the stable Town Hall deployment identifier.</summary>
  public string TownHallId { get; }

  /// <summary>Gets the dedicated SQLite connection string for non-sensitive configuration.</summary>
  public string SqliteConnectionString { get; }

  /// <summary>Creates validated feature-management options.</summary>
  public static SmartCitiesFeatureManagementOptions Create(
    string townHallId,
    string sqliteConnectionString)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      townHallId);
    ArgumentException.ThrowIfNullOrWhiteSpace(
      sqliteConnectionString);

    return new SmartCitiesFeatureManagementOptions(
      townHallId.Trim(),
      sqliteConnectionString.Trim());
  }
}
