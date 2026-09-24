using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SmartCities.Api.Administration;
using SmartCities.Api.Identity;
using SmartCities.Application.Administration;
using SmartCities.Decisions;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Api.Tests.Identity;

public sealed class SmartCitiesAuthorizationTests
{
  [Theory]
  [InlineData("provider-a")]
  [InlineData("provider-b")]
  public async Task Finalize_review_policy_accepts_any_authenticated_provider_after_canonicalization(
    string authenticationType)
  {
    using var provider = BuildServices();
    var authorization = provider.GetRequiredService<
      IAuthorizationService>();
    var principal = CreateCanonicalReviewer(
      authenticationType);

    var result = await authorization.AuthorizeAsync(
      principal,
      resource: null,
      SmartCitiesPolicies.FinalizeDecisionReview);

    Assert.True(result.Succeeded);
  }

  [Fact]
  public async Task Finalize_review_policy_rejects_a_principal_without_the_required_permission()
  {
    using var provider = BuildServices();
    var authorization = provider.GetRequiredService<
      IAuthorizationService>();
    var principal = new ClaimsPrincipal(
      new ClaimsIdentity(
        [
          new Claim(
            SmartCitiesClaimTypes.Subject,
            "reviewer-42"),
          new Claim(
            SmartCitiesClaimTypes.AuthorityRole,
            "mobility-reviewer"),
        ],
        authenticationType: "provider-a"));

    var result = await authorization.AuthorizeAsync(
      principal,
      resource: null,
      SmartCitiesPolicies.FinalizeDecisionReview);

    Assert.False(result.Succeeded);
  }

  [Fact]
  public async Task Provider_native_claims_do_not_bypass_the_canonical_claim_boundary()
  {
    using var provider = BuildServices();
    var authorization = provider.GetRequiredService<
      IAuthorizationService>();
    var principal = new ClaimsPrincipal(
      new ClaimsIdentity(
        [
          new Claim("sub", "reviewer-42"),
          new Claim("role", "mobility-reviewer"),
          new Claim(
            "permission",
            SmartCitiesPermissions.FinalizeDecisionReview),
        ],
        authenticationType: "provider-a"));

    var result = await authorization.AuthorizeAsync(
      principal,
      resource: null,
      SmartCitiesPolicies.FinalizeDecisionReview);

    Assert.False(result.Succeeded);
  }

  [Fact]
  public async Task Feature_configuration_policy_requires_its_canonical_permission()
  {
    using var provider = BuildServices();
    var authorization = provider.GetRequiredService<
      IAuthorizationService>();

    var permitted = new ClaimsPrincipal(
      new ClaimsIdentity(
        [
          new Claim(
            SmartCitiesClaimTypes.Subject,
            "admin-42"),
          new Claim(
            SmartCitiesClaimTypes.AuthorityRole,
            "town-hall-admin"),
          new Claim(
            SmartCitiesClaimTypes.IdentityProvider,
            "provider-a"),
          new Claim(
            SmartCitiesClaimTypes.Permission,
            SmartCitiesPermissions.ConfigureFeatureFlags),
        ],
        authenticationType: "provider-a"));

    var allowed = await authorization.AuthorizeAsync(
      permitted,
      resource: null,
      SmartCitiesPolicies.ConfigureFeatureFlags);

    var denied = await authorization.AuthorizeAsync(
      CreateCanonicalReviewer("provider-a"),
      resource: null,
      SmartCitiesPolicies.ConfigureFeatureFlags);

    Assert.True(allowed.Succeeded);
    Assert.False(denied.Succeeded);
  }

  [Fact]
  public async Task Feature_configuration_permission_does_not_bypass_the_administration_whitelist()
  {
    using var provider = BuildServices(
      administrationAuthorized: false);
    var authorization = provider.GetRequiredService<
      IAuthorizationService>();
    var principal = new ClaimsPrincipal(
      new ClaimsIdentity(
        [
          new Claim(
            SmartCitiesClaimTypes.Subject,
            "admin-42"),
          new Claim(
            SmartCitiesClaimTypes.AuthorityRole,
            "town-hall-admin"),
          new Claim(
            SmartCitiesClaimTypes.IdentityProvider,
            "provider-a"),
          new Claim(
            SmartCitiesClaimTypes.Permission,
            SmartCitiesPermissions.ConfigureFeatureFlags),
        ],
        authenticationType: "provider-a"));

    var result = await authorization.AuthorizeAsync(
      principal,
      resource: null,
      SmartCitiesPolicies.ConfigureFeatureFlags);

    Assert.False(result.Succeeded);
  }

  [Fact]
  public async Task Citizen_mobility_management_requires_global_and_feature_specific_manage_grants()
  {
    using var provider = BuildServices();
    var authorization = provider.GetRequiredService<
      IAuthorizationService>();

    var complete = new ClaimsPrincipal(
      new ClaimsIdentity(
        [
          new Claim(
            SmartCitiesClaimTypes.Subject,
            "operator-42"),
          new Claim(
            SmartCitiesClaimTypes.AuthorityRole,
            "mobility-reviewer"),
          new Claim(
            SmartCitiesClaimTypes.IdentityProvider,
            "provider-a"),
          new Claim(
            SmartCitiesClaimTypes.Permission,
            SmartCitiesPermissions.ManageFeatureFlags),
          new Claim(
            SmartCitiesClaimTypes.Permission,
            SmartCitiesFeaturePermissions.Manage(
              "citizen-mobility")),
        ],
        authenticationType: "provider-a"));

    var globalOnly = new ClaimsPrincipal(
      new ClaimsIdentity(
        [
          new Claim(
            SmartCitiesClaimTypes.Subject,
            "operator-43"),
          new Claim(
            SmartCitiesClaimTypes.AuthorityRole,
            "mobility-reviewer"),
          new Claim(
            SmartCitiesClaimTypes.IdentityProvider,
            "provider-a"),
          new Claim(
            SmartCitiesClaimTypes.Permission,
            SmartCitiesPermissions.ManageFeatureFlags),
        ],
        authenticationType: "provider-a"));

    Assert.True(
      (await authorization.AuthorizeAsync(
        complete,
        resource: null,
        SmartCitiesPolicies.ManageCitizenMobility))
      .Succeeded);
    Assert.False(
      (await authorization.AuthorizeAsync(
        globalOnly,
        resource: null,
        SmartCitiesPolicies.ManageCitizenMobility))
      .Succeeded);
  }

  [Fact]
  public void Canonical_principal_maps_to_the_existing_human_authority_contract()
  {
    var principal = CreateCanonicalReviewer(
      "provider-b");

    HumanAuthority authority =
      principal.ToHumanAuthority();

    Assert.Equal(
      "reviewer-42",
      authority.SubjectId);
    Assert.Equal(
      "mobility-reviewer",
      authority.Role);
  }

  [Fact]
  public void Human_authority_mapping_rejects_ambiguous_canonical_subjects()
  {
    var principal = new ClaimsPrincipal(
      new ClaimsIdentity(
        [
          new Claim(
            SmartCitiesClaimTypes.Subject,
            "reviewer-42"),
          new Claim(
            SmartCitiesClaimTypes.Subject,
            "reviewer-99"),
          new Claim(
            SmartCitiesClaimTypes.AuthorityRole,
            "mobility-reviewer"),
        ],
        authenticationType: "provider-a"));

    Assert.Throws<InvalidOperationException>(
      principal.ToHumanAuthority);
  }

  private static ServiceProvider BuildServices(
    bool administrationAuthorized = true)
  {
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddSingleton<IAdministrationAccessService>(
      new RecordingAdministrationAccessService(
        administrationAuthorized));
    services.AddSmartCitiesAuthorization();
    services.AddSmartCitiesAdministrationAuthorization();

    return services.BuildServiceProvider();
  }

  private sealed class RecordingAdministrationAccessService
    : IAdministrationAccessService
  {
    private readonly bool authorized;

    public RecordingAdministrationAccessService(
      bool authorized)
    {
      this.authorized = authorized;
    }

    public string TownHallId => "test-town-hall";

    public Task<bool> IsAuthorizedAsync(
      AdministrationIdentity identity,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(authorized);
    }

    public Task<bool> IsBootstrapAvailableAsync(
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(false);
    }

    public Task<IReadOnlyList<AdministrationAccessRule>> GetRulesAsync(
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<AdministrationAccessRule> AddRuleAsync(
      AdministrationAccessRuleKind kind,
      string value,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<AdministrationAccessRule> AddRuleAsync(
      AdministrationAccessRuleKind kind,
      string value,
      AdministrationControlPlaneAuditContext auditContext,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<bool> DeleteRuleAsync(
      string ruleId,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<bool> DeleteRuleAsync(
      string ruleId,
      AdministrationControlPlaneAuditContext auditContext,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();
  }

  private static ClaimsPrincipal CreateCanonicalReviewer(
    string authenticationType) =>
    new(
      new ClaimsIdentity(
        [
          new Claim(
            SmartCitiesClaimTypes.Subject,
            "reviewer-42"),
          new Claim(
            SmartCitiesClaimTypes.AuthorityRole,
            "mobility-reviewer"),
          new Claim(
            SmartCitiesClaimTypes.IdentityProvider,
            authenticationType),
          new Claim(
            SmartCitiesClaimTypes.Permission,
            SmartCitiesPermissions.FinalizeDecisionReview),
        ],
        authenticationType));
}
