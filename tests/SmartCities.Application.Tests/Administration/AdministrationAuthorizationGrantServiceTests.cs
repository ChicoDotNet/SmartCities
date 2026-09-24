using SmartCities.Application.Administration;
using SmartCities.Application.FeatureFlags;
using Xunit;

namespace SmartCities.Application.Tests.Administration;

public sealed class AdministrationAuthorizationGrantServiceTests
{
  [Fact]
  public async Task Whitelisted_identity_receives_union_of_matching_domain_email_and_subject_grants()
  {
    var access = new RecordingAccessService(
      authorized: true);
    var store = new RecordingGrantStore(
      [
        AdministrationAuthorizationGrant.Create(
          "grant-domain",
          AdministrationAccessRuleKind.EmailDomain,
          "townhall.gov",
          AdministrationAuthorizationGrantKind.Permission,
          "feature-flags.manage"),
        AdministrationAuthorizationGrant.Create(
          "grant-email",
          AdministrationAccessRuleKind.Email,
          "official@townhall.gov",
          AdministrationAuthorizationGrantKind.Permission,
          "citizen-mobility.manage"),
        AdministrationAuthorizationGrant.Create(
          "grant-subject",
          AdministrationAccessRuleKind.CanonicalSubject,
          "provider:tenant:official-1",
          AdministrationAuthorizationGrantKind.AuthorityRole,
          "mobility-reviewer"),
      ]);
    var service = new AdministrationAuthorizationGrantService(
      new TownHallContext("town-hall-a"),
      access,
      store,
      new RecordingCatalog());

    var grants = await service.GetEffectiveAsync(
      AdministrationIdentity.Create(
        "provider:tenant:official-1",
        "official@townhall.gov"),
      TestContext.Current.CancellationToken);

    Assert.Equal(
      ["mobility-reviewer"],
      grants.AuthorityRoles);
    Assert.Equal(
      [
        "citizen-mobility.manage",
        "feature-flags.manage",
      ],
      grants.Permissions);
  }

  [Fact]
  public async Task Non_whitelisted_identity_receives_no_persisted_grants_even_when_a_target_matches()
  {
    var service = new AdministrationAuthorizationGrantService(
      new TownHallContext("town-hall-a"),
      new RecordingAccessService(
        authorized: false),
      new RecordingGrantStore(
        [
          AdministrationAuthorizationGrant.Create(
            "grant-email",
            AdministrationAccessRuleKind.Email,
            "official@example.com",
            AdministrationAuthorizationGrantKind.Permission,
            "feature-flags.config"),
        ]),
      new RecordingCatalog());

    var grants = await service.GetEffectiveAsync(
      AdministrationIdentity.Create(
        "provider:tenant:official",
        "official@example.com"),
      TestContext.Current.CancellationToken);

    Assert.Empty(grants.AuthorityRoles);
    Assert.Empty(grants.Permissions);
  }

  [Fact]
  public async Task Grant_creation_rejects_unknown_roles_and_permissions()
  {
    var service = new AdministrationAuthorizationGrantService(
      new TownHallContext("town-hall-a"),
      new RecordingAccessService(
        authorized: true),
      new RecordingGrantStore([]),
      new RecordingCatalog());

    await Assert.ThrowsAsync<ArgumentException>(
      () => service.AddAsync(
        AdministrationAccessRuleKind.Email,
        "official@example.com",
        AdministrationAuthorizationGrantKind.Permission,
        "root.everything",
        TestContext.Current.CancellationToken));

    await Assert.ThrowsAsync<ArgumentException>(
      () => service.AddAsync(
        AdministrationAccessRuleKind.Email,
        "official@example.com",
        AdministrationAuthorizationGrantKind.AuthorityRole,
        "superuser",
        TestContext.Current.CancellationToken));
  }

  [Fact]
  public async Task Identical_normalized_assignment_is_idempotent()
  {
    var store = new RecordingGrantStore([]);
    var service = new AdministrationAuthorizationGrantService(
      new TownHallContext("town-hall-a"),
      new RecordingAccessService(
        authorized: true),
      store,
      new RecordingCatalog());

    var first = await service.AddAsync(
      AdministrationAccessRuleKind.EmailDomain,
      "@TownHall.GOV",
      AdministrationAuthorizationGrantKind.Permission,
      "feature-flags.manage",
      TestContext.Current.CancellationToken);
    var second = await service.AddAsync(
      AdministrationAccessRuleKind.EmailDomain,
      "townhall.gov",
      AdministrationAuthorizationGrantKind.Permission,
      "feature-flags.manage",
      TestContext.Current.CancellationToken);

    Assert.Equal(first.GrantId, second.GrantId);
    Assert.Single(
      await store.GetAllAsync(
        "town-hall-a",
        TestContext.Current.CancellationToken));
  }

  private sealed class RecordingAccessService(
    bool authorized)
    : IAdministrationAccessService
  {
    public string TownHallId => "town-hall-a";

    public Task<bool> IsAuthorizedAsync(
      AdministrationIdentity identity,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(authorized);
    }

    public Task<bool> IsBootstrapAvailableAsync(
      CancellationToken cancellationToken = default) =>
      Task.FromResult(false);

    public Task<IReadOnlyList<AdministrationAccessRule>>
      GetRulesAsync(
        CancellationToken cancellationToken = default) =>
      Task.FromResult<IReadOnlyList<AdministrationAccessRule>>([]);

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

  private sealed class RecordingGrantStore(
    IReadOnlyList<AdministrationAuthorizationGrant> seed)
    : IAdministrationAuthorizationGrantStore
  {
    private readonly List<AdministrationAuthorizationGrant> grants =
      [.. seed];

    public Task<IReadOnlyList<AdministrationAuthorizationGrant>>
      GetAllAsync(
        string townHallId,
        CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult<IReadOnlyList<AdministrationAuthorizationGrant>>(
        grants.ToArray());
    }

    public Task AddAsync(
      string townHallId,
      AdministrationAuthorizationGrant grant,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      grants.Add(grant);
      return Task.CompletedTask;
    }

    public Task AddAsync(
      string townHallId,
      AdministrationAuthorizationGrant grant,
      AdministrationControlPlaneAuditEvent auditEvent,
      CancellationToken cancellationToken = default)
    {
      ArgumentNullException.ThrowIfNull(auditEvent);
      return AddAsync(
        townHallId,
        grant,
        cancellationToken);
    }

    public Task<bool> DeleteAsync(
      string townHallId,
      string grantId,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(
        grants.RemoveAll(
          item => string.Equals(
            item.GrantId,
            grantId,
            StringComparison.Ordinal)) == 1);
    }

    public Task<bool> DeleteAsync(
      string townHallId,
      string grantId,
      AdministrationControlPlaneAuditEvent auditEvent,
      CancellationToken cancellationToken = default)
    {
      ArgumentNullException.ThrowIfNull(auditEvent);
      return DeleteAsync(
        townHallId,
        grantId,
        cancellationToken);
    }
  }

  private sealed class RecordingCatalog
    : IAdministrationAuthorizationGrantCatalog
  {
    public IReadOnlyList<string> AuthorityRoles =>
      ["mobility-reviewer", "town-hall-admin"];

    public IReadOnlyList<string> Permissions =>
      [
        "administration-grants.manage",
        "citizen-mobility.config",
        "citizen-mobility.manage",
        "feature-flags.config",
        "feature-flags.manage",
      ];
  }
}
