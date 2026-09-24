using SmartCities.Application.Administration;
using SmartCities.Application.FeatureFlags;
using Xunit;

namespace SmartCities.Application.Tests.Administration;

public sealed class AdministrationAccessServiceTests
{
  [Fact]
  public async Task Bootstrap_admin_is_admitted_only_while_the_whitelist_is_empty()
  {
    var store = new RecordingAdministrationAccessRuleStore();
    var service = new AdministrationAccessService(
      new TownHallContext("town-hall-a"),
      store);

    var before = await service.IsAuthorizedAsync(
      AdministrationIdentity.Create(
        AdministrationAccessService.BootstrapCanonicalSubject,
        "townhalladmin@smartcities.local"),
      TestContext.Current.CancellationToken);

    await store.AddAsync(
      "town-hall-a",
      AdministrationAccessRule.Create(
        "rule-001",
        AdministrationAccessRuleKind.CanonicalSubject,
        "provider-a:tenant-a:official-001"),
      TestContext.Current.CancellationToken);

    var after = await service.IsAuthorizedAsync(
      AdministrationIdentity.Create(
        AdministrationAccessService.BootstrapCanonicalSubject,
        "townhalladmin@smartcities.local"),
      TestContext.Current.CancellationToken);

    Assert.True(before);
    Assert.False(after);
  }

  [Theory]
  [InlineData("Official@TownHallName.GOB.MX", "townhallname.gob.mx")]
  [InlineData("person@townhallname.gov", "@TOWNHALLNAME.GOV")]
  public async Task Domain_rules_match_canonical_email_case_insensitively(
    string email,
    string configuredDomain)
  {
    var store = new RecordingAdministrationAccessRuleStore();
    await store.AddAsync(
      "town-hall-a",
      AdministrationAccessRule.Create(
        "rule-domain",
        AdministrationAccessRuleKind.EmailDomain,
        configuredDomain),
      TestContext.Current.CancellationToken);

    var service = new AdministrationAccessService(
      new TownHallContext("town-hall-a"),
      store);

    Assert.True(
      await service.IsAuthorizedAsync(
        AdministrationIdentity.Create(
          "provider-a:tenant-a:official-001",
          email),
        TestContext.Current.CancellationToken));
  }

  [Fact]
  public async Task Exact_email_rule_supports_a_personal_official_email()
  {
    var store = new RecordingAdministrationAccessRuleStore();
    await store.AddAsync(
      "town-hall-a",
      AdministrationAccessRule.Create(
        "rule-email",
        AdministrationAccessRuleKind.Email,
        "Official.Person@Example.com"),
      TestContext.Current.CancellationToken);

    var service = new AdministrationAccessService(
      new TownHallContext("town-hall-a"),
      store);

    Assert.True(
      await service.IsAuthorizedAsync(
        AdministrationIdentity.Create(
          "google:default:abc",
          "official.person@example.com"),
        TestContext.Current.CancellationToken));
  }

  [Fact]
  public async Task Canonical_subject_rule_supports_providers_without_email()
  {
    var store = new RecordingAdministrationAccessRuleStore();
    await store.AddAsync(
      "town-hall-a",
      AdministrationAccessRule.Create(
        "rule-subject",
        AdministrationAccessRuleKind.CanonicalSubject,
        "provider-x:tenant-17:subject-123"),
      TestContext.Current.CancellationToken);

    var service = new AdministrationAccessService(
      new TownHallContext("town-hall-a"),
      store);

    Assert.True(
      await service.IsAuthorizedAsync(
        AdministrationIdentity.Create(
          "provider-x:tenant-17:subject-123",
          emailAddress: null),
        TestContext.Current.CancellationToken));
  }

  [Fact]
  public async Task Rules_are_isolated_by_Town_Hall()
  {
    var store = new RecordingAdministrationAccessRuleStore();
    await store.AddAsync(
      "town-hall-a",
      AdministrationAccessRule.Create(
        "rule-a",
        AdministrationAccessRuleKind.EmailDomain,
        "town-a.gov"),
      TestContext.Current.CancellationToken);

    var first = new AdministrationAccessService(
      new TownHallContext("town-hall-a"),
      store);
    var second = new AdministrationAccessService(
      new TownHallContext("town-hall-b"),
      store);
    var identity = AdministrationIdentity.Create(
      "provider-a:tenant:subject",
      "official@town-a.gov");

    Assert.True(
      await first.IsAuthorizedAsync(
        identity,
        TestContext.Current.CancellationToken));
    Assert.False(
      await second.IsAuthorizedAsync(
        identity,
        TestContext.Current.CancellationToken));
  }

  [Theory]
  [InlineData(AdministrationAccessRuleKind.EmailDomain, "*.town.gov")]
  [InlineData(AdministrationAccessRuleKind.EmailDomain, "town.gov/path")]
  [InlineData(AdministrationAccessRuleKind.Email, "not-an-email")]
  [InlineData(AdministrationAccessRuleKind.CanonicalSubject, " ")]
  public void Invalid_or_ambiguous_rules_are_rejected(
    AdministrationAccessRuleKind kind,
    string value)
  {
    Assert.Throws<ArgumentException>(
      () => AdministrationAccessRule.Create(
        "rule-invalid",
        kind,
        value));
  }

  private sealed class RecordingAdministrationAccessRuleStore
    : IAdministrationAccessRuleStore
  {
    private readonly Dictionary<
      string,
      List<AdministrationAccessRule>> rules =
        new(StringComparer.Ordinal);

    public Task<IReadOnlyList<AdministrationAccessRule>>
      GetAllAsync(
        string townHallId,
        CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      return Task.FromResult<
        IReadOnlyList<AdministrationAccessRule>>(
          rules.TryGetValue(townHallId, out var values)
            ? values.ToArray()
            : []);
    }

    public Task AddAsync(
      string townHallId,
      AdministrationAccessRule rule,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (!rules.TryGetValue(townHallId, out var values))
      {
        values = [];
        rules.Add(townHallId, values);
      }

      values.Add(rule);
      return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(
      string townHallId,
      string ruleId,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var deleted =
        rules.TryGetValue(townHallId, out var values)
        && values.RemoveAll(
          rule => string.Equals(
            rule.RuleId,
            ruleId,
            StringComparison.Ordinal)) == 1;

      return Task.FromResult(deleted);
    }
  }
}
