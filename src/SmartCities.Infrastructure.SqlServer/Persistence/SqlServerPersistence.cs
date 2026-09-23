using Microsoft.EntityFrameworkCore;
using SmartCities.Infrastructure.Persistence;

namespace SmartCities.Infrastructure.SqlServer.Persistence;

/// <summary>
/// Configures <see cref="SmartCitiesDbContext"/> for Microsoft SQL Server with an isolated migration chain.
/// </summary>
public static class SqlServerPersistence
{
  /// <summary>Gets the relational provider represented by this adapter.</summary>
  public static DbProvider Provider => DbProvider.SqlServer;

  /// <summary>
  /// Configures the supplied options builder for SQL Server and the SQL Server-specific migrations assembly.
  /// </summary>
  /// <param name="builder">DbContext options builder owned by the composition root.</param>
  /// <param name="connectionString">Non-empty SQL Server connection string.</param>
  /// <returns>The same builder for composition chaining.</returns>
  public static DbContextOptionsBuilder<SmartCitiesDbContext> Configure(
    DbContextOptionsBuilder<SmartCitiesDbContext> builder,
    string connectionString)
  {
    ArgumentNullException.ThrowIfNull(builder);
    ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

    var migrationsAssembly = typeof(SqlServerPersistence).Assembly.GetName().Name
      ?? throw new InvalidOperationException("Unable to resolve the SQL Server migrations assembly name.");

    builder.UseSqlServer(
      connectionString,
      options => options.MigrationsAssembly(migrationsAssembly));

    return builder;
  }
}
