namespace SmartCities.Api.Administration;

/// <summary>
/// Requests one persisted Administration authorization grant.
/// </summary>
/// <param name="TargetKind">email-domain, email, or canonical-subject.</param>
/// <param name="TargetValue">Portable selector value.</param>
/// <param name="GrantKind">authority-role or permission.</param>
/// <param name="Value">Code-owned canonical role or permission value.</param>
public sealed record AddAdministrationAuthorizationGrantRequest(
  string TargetKind,
  string TargetValue,
  string GrantKind,
  string Value);
