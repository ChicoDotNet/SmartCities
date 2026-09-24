using Microsoft.AspNetCore.Authorization;
using SmartCities.Api.Administration;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Api.Tests.Administration;

public sealed class AdministrationControlPlaneAuditControllerTests
{
  [Fact]
  public void Audit_history_requires_the_canonical_audit_read_policy()
  {
    var method = typeof(AdministrationControlPlaneAuditController)
      .GetMethod(nameof(
        AdministrationControlPlaneAuditController.GetAsync));

    var authorize = Assert.Single(
      method!.GetCustomAttributes(
        typeof(AuthorizeAttribute),
        inherit: true)
      .Cast<AuthorizeAttribute>());

    Assert.Equal(
      SmartCitiesPolicies.ReadAdministrationAudit,
      authorize.Policy);
  }
}
