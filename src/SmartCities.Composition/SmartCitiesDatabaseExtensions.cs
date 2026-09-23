using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Infrastructure.Persistence;

namespace SmartCities.Composition;

/// <summary>
/// Provides explicit relational schema lifecycle operations for SmartCities hosts and deployment tooling.
/// </summary>
public static class SmartCitiesDatabaseExtensions
{
  /// <summary>
  /// Applies the selected provider's pending EF Core migrations.
  /// </summary>
  /// <param name="services">Configured SmartCities service provider.</param>
  /// <param name="cancellationToken">Token used to cancel migration work.</param>
  /// <returns>A task that completes when pending migrations have been applied.</returns>
  /// <remarks>
  /// Production hosts should invoke this only through their controlled deployment strategy. The API host uses
  /// this operation automatically only in the Development environment when explicitly enabled by configuration.
  /// </remarks>
  public static async Task ApplySmartCitiesMigrationsAsync(
    this IServiceProvider services,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(services);

    await using var scope = services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider
      .GetRequiredService<SmartCitiesDbContext>();

    await dbContext.Database
      .MigrateAsync(cancellationToken)
      .ConfigureAwait(false);
  }
}
