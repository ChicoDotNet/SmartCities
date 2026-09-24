namespace SmartCities.Application.Administration;

/// <summary>
/// Provides append-only persistence and bounded history reads for Town Hall control-plane audit events.
/// </summary>
public interface IAdministrationControlPlaneAuditStore
{
  /// <summary>Appends one immutable audit event.</summary>
  Task AppendAsync(
    AdministrationControlPlaneAuditEvent auditEvent,
    CancellationToken cancellationToken = default);

  /// <summary>Gets newest-first recent audit events for one Town Hall.</summary>
  Task<IReadOnlyList<AdministrationControlPlaneAuditEvent>> GetRecentAsync(
    string townHallId,
    int limit,
    CancellationToken cancellationToken = default);
}
