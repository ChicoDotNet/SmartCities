namespace SmartCities.Api.Identity;

/// <summary>
/// Describes the authoritative canonical authentication state for the current request.
/// </summary>
public sealed record AuthenticationSessionResponse(
  bool Authenticated,
  string? SubjectId,
  string? IdentityProvider,
  IReadOnlyList<string> AuthorityRoles,
  IReadOnlyList<string> Permissions);
