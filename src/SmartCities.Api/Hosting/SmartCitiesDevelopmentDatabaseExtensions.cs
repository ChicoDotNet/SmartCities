using SmartCities.Composition;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Controls development-only automatic database migration for the executable local vertical slice.
/// </summary>
public static class SmartCitiesDevelopmentDatabaseExtensions
{
  /// <summary>Configuration key enabling development-only migration on API startup.</summary>
  public const string ApplyMigrationsOnStartupKey =
    "SmartCities:Persistence:ApplyMigrationsOnStartup";

  /// <summary>
  /// Applies pending SmartCities migrations only when the host is running in Development and startup migration
  /// has been explicitly enabled.
  /// </summary>
  /// <param name="app">Built ASP.NET Core application.</param>
  /// <param name="cancellationToken">Token used to cancel startup migration work.</param>
  /// <returns>A task that completes after the optional development migration step.</returns>
  public static async Task ApplySmartCitiesDevelopmentDatabaseAsync(
    this WebApplication app,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(app);

    if (!app.Environment.IsDevelopment())
    {
      return;
    }

    var configuredValue =
      app.Configuration[ApplyMigrationsOnStartupKey];

    if (!bool.TryParse(
        configuredValue,
        out var applyMigrations)
      || !applyMigrations)
    {
      return;
    }

    await app.Services
      .ApplySmartCitiesMigrationsAsync(cancellationToken)
      .ConfigureAwait(false);
  }
}
