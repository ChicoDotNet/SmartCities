namespace SmartCities.Application.FeatureFlags;

/// <summary>
/// Resolves and persists effective vertical-slice feature flags for the current Town Hall.
/// </summary>
public interface IFeatureFlagService
{
  /// <summary>Gets the current Town Hall identifier.</summary>
  string TownHallId { get; }

  /// <summary>Gets all known feature states in deterministic order.</summary>
  Task<IReadOnlyList<FeatureFlagState>> GetAllAsync(
    CancellationToken cancellationToken = default);

  /// <summary>Gets one known feature state, or null when the feature identifier is unknown.</summary>
  Task<FeatureFlagState?> GetAsync(
    string featureId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Persists one known feature override, or returns null when the feature identifier is unknown.
  /// </summary>
  Task<FeatureFlagState?> SetAsync(
    string featureId,
    bool enabled,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Persists one known feature override with immutable control-plane audit context.
  /// </summary>
  Task<FeatureFlagState?> SetAsync(
    string featureId,
    bool enabled,
    Administration.AdministrationControlPlaneAuditContext auditContext,
    CancellationToken cancellationToken = default);
}
