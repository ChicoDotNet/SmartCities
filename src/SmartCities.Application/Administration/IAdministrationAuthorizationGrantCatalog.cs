namespace SmartCities.Application.Administration;

/// <summary>
/// Enumerates the code-owned canonical roles and permissions that may be persisted as Town Hall grants.
/// </summary>
public interface IAdministrationAuthorizationGrantCatalog
{
  /// <summary>Gets assignable canonical authority roles.</summary>
  IReadOnlyList<string> AuthorityRoles { get; }

  /// <summary>Gets assignable canonical permissions.</summary>
  IReadOnlyList<string> Permissions { get; }
}
