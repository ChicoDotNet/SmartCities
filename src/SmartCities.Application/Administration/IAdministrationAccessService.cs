namespace SmartCities.Application.Administration;

/// <summary>
/// Evaluates and manages Town Hall Administration admission.
/// </summary>
public interface IAdministrationAccessService
{
  /// <summary>Gets the current Town Hall identifier.</summary>
  string TownHallId { get; }

  /// <summary>Determines whether a canonical identity may enter Town Hall Administration.</summary>
  Task<bool> IsAuthorizedAsync(
    AdministrationIdentity identity,
    CancellationToken cancellationToken = default);

  /// <summary>Determines whether the bootstrap administration identity is currently admissible.</summary>
  Task<bool> IsBootstrapAvailableAsync(
    CancellationToken cancellationToken = default);

  /// <summary>Gets all configured admission rules.</summary>
  Task<IReadOnlyList<AdministrationAccessRule>> GetRulesAsync(
    CancellationToken cancellationToken = default);

  /// <summary>Adds or reuses one normalized admission rule.</summary>
  Task<AdministrationAccessRule> AddRuleAsync(
    AdministrationAccessRuleKind kind,
    string value,
    CancellationToken cancellationToken = default);

  /// <summary>Deletes one configured admission rule.</summary>
  Task<bool> DeleteRuleAsync(
    string ruleId,
    CancellationToken cancellationToken = default);
}
