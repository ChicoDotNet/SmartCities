namespace SmartCities.Api.Administration;

/// <summary>
/// Enumerates the code-owned role and permission values assignable by Town Hall Administration.
/// </summary>
/// <param name="AuthorityRoles">Assignable canonical authority roles.</param>
/// <param name="Permissions">Assignable canonical permissions.</param>
public sealed record AdministrationAuthorizationGrantCatalogResponse(
  IReadOnlyList<string> AuthorityRoles,
  IReadOnlyList<string> Permissions);
