using Microsoft.AspNetCore.Authorization;
using SmartCities.Api.Administration;
using SmartCities.Identity;

namespace SmartCities.Api.Identity;

/// <summary>
/// Registers SmartCities authorization policies over provider-neutral canonical claims.
/// </summary>
public static class SmartCitiesAuthorizationExtensions
{
  /// <summary>
  /// Registers provider-neutral SmartCities authorization policies.
  /// </summary>
  /// <param name="services">Host dependency-injection services.</param>
  /// <returns>The same service collection for composition chaining.</returns>
  /// <remarks>
  /// This method does not register an authentication scheme. Authentication-provider adapters are responsible for
  /// validating identities and normalizing their external claims into the canonical SmartCities claim types.
  /// </remarks>
  public static IServiceCollection AddSmartCitiesAuthorization(
    this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddAuthorization(
      options =>
      {
        options.AddPolicy(
          SmartCitiesPolicies.FinalizeDecisionReview,
          policy =>
          {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(
              SmartCitiesClaimTypes.Subject);
            policy.RequireClaim(
              SmartCitiesClaimTypes.AuthorityRole);
            policy.RequireClaim(
              SmartCitiesClaimTypes.Permission,
              SmartCitiesPermissions.FinalizeDecisionReview);
          });

        options.AddPolicy(
          SmartCitiesPolicies.TownHallAdministrationAccess,
          policy =>
          {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(
              SmartCitiesClaimTypes.Subject);
            policy.AddRequirements(
              new AdministrationAccessRequirement());
          });

        options.AddPolicy(
          SmartCitiesPolicies.ManageFeatureFlags,
          policy =>
          {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(
              SmartCitiesClaimTypes.Subject);
            policy.RequireClaim(
              SmartCitiesClaimTypes.AuthorityRole);
            policy.RequireClaim(
              SmartCitiesClaimTypes.Permission,
              SmartCitiesPermissions.ManageFeatureFlags);
            policy.AddRequirements(
              new AdministrationAccessRequirement());
          });

        options.AddPolicy(
          SmartCitiesPolicies.ManageAdministrationWhitelist,
          policy =>
          {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(
              SmartCitiesClaimTypes.Subject);
            policy.RequireClaim(
              SmartCitiesClaimTypes.AuthorityRole);
            policy.RequireClaim(
              SmartCitiesClaimTypes.Permission,
              SmartCitiesPermissions.ManageAdministrationWhitelist);
            policy.AddRequirements(
              new AdministrationAccessRequirement());
          });
      });

    return services;
  }
}
