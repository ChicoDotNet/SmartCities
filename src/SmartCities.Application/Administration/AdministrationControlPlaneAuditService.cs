using SmartCities.Application.FeatureFlags;

namespace SmartCities.Application.Administration;

/// <summary>
/// Reads bounded recent control-plane audit history for one Town Hall deployment.
/// </summary>
public sealed class AdministrationControlPlaneAuditService
  : IAdministrationControlPlaneAuditService
{
  private const int MaximumLimit = 200;

  private readonly TownHallContext townHall;
  private readonly IAdministrationControlPlaneAuditStore store;

  /// <summary>Initializes the audit-history service.</summary>
  public AdministrationControlPlaneAuditService(
    TownHallContext townHall,
    IAdministrationControlPlaneAuditStore store)
  {
    ArgumentNullException.ThrowIfNull(townHall);
    ArgumentNullException.ThrowIfNull(store);
    this.townHall = townHall;
    this.store = store;
  }

  /// <inheritdoc />
  public string TownHallId => townHall.TownHallId;

  /// <inheritdoc />
  public Task<IReadOnlyList<AdministrationControlPlaneAuditEvent>>
    GetRecentAsync(
      int limit,
      CancellationToken cancellationToken = default)
  {
    if (limit is < 1 or > MaximumLimit)
    {
      throw new ArgumentOutOfRangeException(
        nameof(limit),
        limit,
        $"Audit history limit must be between 1 and {MaximumLimit}.");
    }

    return store.GetRecentAsync(
      TownHallId,
      limit,
      cancellationToken);
  }
}
