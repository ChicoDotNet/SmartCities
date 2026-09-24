namespace SmartCities.Application.Administration;

/// <summary>
/// Persists typed per-Town-Hall Administration authorization grants.
/// </summary>
public interface IAdministrationAuthorizationGrantStore
{
  /// <summary>Gets all persisted grants for one Town Hall.</summary>
  Task<IReadOnlyList<AdministrationAuthorizationGrant>> GetAllAsync(
    string townHallId,
    CancellationToken cancellationToken = default);

  /// <summary>Adds one grant for one Town Hall.</summary>
  Task AddAsync(
    string townHallId,
    AdministrationAuthorizationGrant grant,
    CancellationToken cancellationToken = default);

  /// <summary>Adds one grant and its completed audit event atomically.</summary>
  Task AddAsync(
    string townHallId,
    AdministrationAuthorizationGrant grant,
    AdministrationControlPlaneAuditEvent auditEvent,
    CancellationToken cancellationToken = default);

  /// <summary>Deletes one grant by identifier within one Town Hall.</summary>
  Task<bool> DeleteAsync(
    string townHallId,
    string grantId,
    CancellationToken cancellationToken = default);

  /// <summary>Deletes one grant and appends its audit event in the same transaction when the grant exists.</summary>
  Task<bool> DeleteAsync(
    string townHallId,
    string grantId,
    AdministrationControlPlaneAuditEvent auditEvent,
    CancellationToken cancellationToken = default);
}
