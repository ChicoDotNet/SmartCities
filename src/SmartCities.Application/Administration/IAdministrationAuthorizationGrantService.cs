namespace SmartCities.Application.Administration;

/// <summary>
/// Resolves and manages persisted authorization grants for admitted Town Hall identities.
/// </summary>
public interface IAdministrationAuthorizationGrantService
{
  /// <summary>Gets the current Town Hall identifier.</summary>
  string TownHallId { get; }

  /// <summary>Gets persisted grants effective for one currently admitted canonical identity.</summary>
  Task<AdministrationEffectiveGrants> GetEffectiveAsync(
    AdministrationIdentity identity,
    CancellationToken cancellationToken = default);

  /// <summary>Gets all persisted grant assignments for the current Town Hall.</summary>
  Task<IReadOnlyList<AdministrationAuthorizationGrant>> GetAllAsync(
    CancellationToken cancellationToken = default);

  /// <summary>Adds or reuses one validated grant assignment.</summary>
  Task<AdministrationAuthorizationGrant> AddAsync(
    AdministrationAccessRuleKind targetKind,
    string targetValue,
    AdministrationAuthorizationGrantKind kind,
    string value,
    CancellationToken cancellationToken = default);

  /// <summary>Deletes one grant assignment.</summary>
  Task<bool> DeleteAsync(
    string grantId,
    CancellationToken cancellationToken = default);
}
