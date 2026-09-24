namespace SmartCities.Api.Administration;

/// <summary>
/// Represents one persisted Town Hall authorization grant assignment.
/// </summary>
/// <param name="GrantId">Stable grant identifier.</param>
/// <param name="TargetKind">Portable identity selector kind.</param>
/// <param name="TargetValue">Normalized selector value.</param>
/// <param name="GrantKind">Canonical grant kind.</param>
/// <param name="Value">Canonical role or permission value.</param>
public sealed record AdministrationAuthorizationGrantResponse(
  string GrantId,
  string TargetKind,
  string TargetValue,
  string GrantKind,
  string Value);
