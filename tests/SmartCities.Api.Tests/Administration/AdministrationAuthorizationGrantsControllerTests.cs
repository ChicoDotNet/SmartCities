using Microsoft.AspNetCore.Authorization;
using SmartCities.Api.Administration;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Api.Tests.Administration;

public sealed class AdministrationAuthorizationGrantsControllerTests
{
  [Theory]
  [InlineData(nameof(AdministrationAuthorizationGrantsController.GetAsync))]
  [InlineData(nameof(AdministrationAuthorizationGrantsController.GetCatalog))]
  [InlineData(nameof(AdministrationAuthorizationGrantsController.AddAsync))]
  [InlineData(nameof(AdministrationAuthorizationGrantsController.DeleteAsync))]
  public void Grant_management_requires_the_canonical_grant_management_policy(
    string methodName)
  {
    var method = typeof(AdministrationAuthorizationGrantsController)
      .GetMethod(methodName);

    var authorize = Assert.Single(
      method!.GetCustomAttributes(
        typeof(AuthorizeAttribute),
        inherit: true)
      .Cast<AuthorizeAttribute>());

    Assert.Equal(
      SmartCitiesPolicies.ManageAdministrationGrants,
      authorize.Policy);
  }
}
