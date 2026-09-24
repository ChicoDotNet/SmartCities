namespace SmartCities.Api.Identity;

/// <summary>
/// Represents the canonical identity established in the local browser session.
/// </summary>
public sealed record LocalSessionResponse(
  string SubjectId,
  string IdentityProvider,
  IReadOnlyList<string> AuthorityRoles);
