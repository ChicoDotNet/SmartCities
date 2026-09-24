namespace SmartCities.Application.Administration;

/// <summary>
/// Persists per-Town-Hall administration admission rules.
/// </summary>
public interface IAdministrationAccessRuleStore
{
  /// <summary>Gets all rules for one Town Hall.</summary>
  Task<IReadOnlyList<AdministrationAccessRule>> GetAllAsync(
    string townHallId,
    CancellationToken cancellationToken = default);

  /// <summary>Adds one rule for one Town Hall.</summary>
  Task AddAsync(
    string townHallId,
    AdministrationAccessRule rule,
    CancellationToken cancellationToken = default);

  /// <summary>Deletes one rule by identifier within one Town Hall.</summary>
  Task<bool> DeleteAsync(
    string townHallId,
    string ruleId,
    CancellationToken cancellationToken = default);
}
