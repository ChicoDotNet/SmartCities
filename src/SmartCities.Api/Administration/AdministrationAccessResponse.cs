namespace SmartCities.Api.Administration;

/// <summary>
/// Describes whether the current canonical identity may enter Town Hall Administration.
/// </summary>
/// <param name="TownHallId">Current Town Hall deployment identifier.</param>
/// <param name="Authorized">Whether the current request identity is admitted.</param>
/// <param name="BootstrapAvailable">Whether the empty-whitelist bootstrap path is currently available.</param>
public sealed record AdministrationAccessResponse(
  string TownHallId,
  bool Authorized,
  bool BootstrapAvailable);
