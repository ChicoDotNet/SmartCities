using Microsoft.EntityFrameworkCore;
using SmartCities.Infrastructure.Persistence;

namespace SmartCities.Infrastructure.PostgreSql.Persistence;

/// <summary>
/// Configures <see cref="SmartCitiesDbContext"/> for PostgreSQL with an isolated migration chain.
/// </summary>
public static class PostgreSqlPersistence
{
  /// <summary>Gets the relational provider represented by this adapter.</summary>
  public static DbProvider Provider => DbProvider.PostgreSql;

  /// <summary>
  /// Configures the supplied options builder for PostgreSQL and the PostgreSQL-specific migrations assembly.
  /// </summary>
  /// <param name="builder">DbContext options builder owned by the composition root.</param>
  /// <param name="connectionString">Non-empty PostgreSQL connection string.</param>
  /// <returns>The same builder for composition chaining.</returns>
  public static DbContextOptionsBuilder<SmartCitiesDbContext> Configure(
    DbContextOptionsBuilder<SmartCitiesDbContext> builder,
    string connectionString)
  {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

    var migrationsAssembly = typeof(PostgreSqlPersistence).Assembly.GetName().Name
      ?? throw new InvalidOperationException("Unable to resolve the PostgreSQL migrations assembly name.");

    builder.UseNpgsql(
      connectionString,
      options => options.MigrationsAssembly(migrationsAssembly));

    return builder;
  }
}
