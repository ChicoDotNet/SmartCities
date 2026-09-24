namespace SmartCities.Application.Administration;

/// <summary>
/// Reads bounded append-only control-plane audit history for the current Town Hall.
/// </summary>
public interface IAdministrationControlPlaneAuditService
{
  /// <summary>Gets the current Town Hall identifier.</summary>
  string TownHallId { get; }

  /// <summary>Gets newest-first recent audit events.</summary>
  Task<IReadOnlyList<AdministrationControlPlaneAuditEvent>> GetRecentAsync(
    int limit,
    CancellationToken cancellationToken = default);
}
