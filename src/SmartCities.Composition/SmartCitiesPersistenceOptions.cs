using SmartCities.Infrastructure.Persistence;

namespace SmartCities.Composition;

/// <summary>
/// Represents validated host configuration for the SmartCities relational persistence boundary.
/// </summary>
public sealed record SmartCitiesPersistenceOptions
{
  private SmartCitiesPersistenceOptions(
    DbProvider provider,
    string connectionString)
  {
    Provider = provider;
    ConnectionString = connectionString;
  }

  /// <summary>Gets the selected production relational provider.</summary>
  public DbProvider Provider { get; }

  /// <summary>Gets the provider-specific connection string supplied by the host.</summary>
  public string ConnectionString { get; }

  /// <summary>Creates validated persistence configuration for the composition root.</summary>
  /// <param name="provider">Supported production relational provider.</param>
  /// <param name="connectionString">Non-empty provider-specific connection string.</param>
  /// <returns>Validated immutable persistence options.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="provider"/> is unsupported.</exception>
  /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is empty or whitespace.</exception>
  public static SmartCitiesPersistenceOptions Create(
    DbProvider provider,
    string connectionString)
  {
    if (!Enum.IsDefined(provider))
    {
      throw new ArgumentOutOfRangeException(
        nameof(provider),
        provider,
        "Unsupported SmartCities relational provider.");
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

    return new SmartCitiesPersistenceOptions(
      provider,
      connectionString);
  }
}
