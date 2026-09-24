using System.Security.Claims;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Identity.Tests;

public sealed class CanonicalEmailIdentityTests
{
  [Fact]
  public void Canonical_email_round_trips_through_the_framework_principal()
  {
    var canonical = CanonicalIdentity.Create(
      identityProvider: "provider-a",
      subjectId: "provider-a:tenant-a:subject-001",
      authorityRoles: [],
      permissions: [],
      emailAddress: "Official.Person@TownHall.GOV");

    ClaimsPrincipal principal = canonical.ToClaimsPrincipal(
      "provider-a");

    Assert.Equal(
      "official.person@townhall.gov",
      Assert.Single(
        principal.FindAll(
          SmartCitiesClaimTypes.EmailAddress)).Value);

    var recovered = principal.ToCanonicalIdentity();

    Assert.Equal(
      "official.person@townhall.gov",
      recovered.EmailAddress);
  }

  [Fact]
  public void Canonical_email_rejects_non_email_values()
  {
    Assert.Throws<ArgumentException>(
      () => CanonicalIdentity.Create(
        "provider-a",
        "provider-a:default:subject",
        [],
        [],
        emailAddress: "not-an-email"));
  }
}
