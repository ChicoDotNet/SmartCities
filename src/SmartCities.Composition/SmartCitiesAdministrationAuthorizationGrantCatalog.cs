using SmartCities.Application.Administration;
using SmartCities.Application.FeatureFlags;
using SmartCities.Identity;

namespace SmartCities.Composition;

internal sealed class SmartCitiesAdministrationAuthorizationGrantCatalog
  : IAdministrationAuthorizationGrantCatalog
{
  private static readonly IReadOnlyList<string> Roles =
    SmartCitiesAuthorityRoles.All
      .Order(StringComparer.Ordinal)
      .ToArray();

  private static readonly IReadOnlyList<string> PermissionValues =
    new[]
    {
      SmartCitiesPermissions.FinalizeDecisionReview,
      SmartCitiesPermissions.ManageFeatureFlags,
      SmartCitiesPermissions.ConfigureFeatureFlags,
      SmartCitiesPermissions.ManageAdministrationWhitelist,
      SmartCitiesPermissions.ManageAdministrationGrants,
      SmartCitiesPermissions.ReadAdministrationAudit,
    }
    .Concat(
      SmartCitiesFeatures.All.SelectMany(
        static featureId =>
          new[]
          {
            SmartCitiesFeaturePermissions.Manage(featureId),
            SmartCitiesFeaturePermissions.Configure(featureId),
          }))
    .Distinct(StringComparer.Ordinal)
    .Order(StringComparer.Ordinal)
    .ToArray();

  public IReadOnlyList<string> AuthorityRoles =>
    Roles;

  public IReadOnlyList<string> Permissions =>
    PermissionValues;
}
