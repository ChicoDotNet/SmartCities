using SmartCities.Application.FeatureFlags;

namespace SmartCities.Application.Administration;

/// <summary>
/// Evaluates portable Town Hall Administration admission rules against canonical identity material.
/// </summary>
public sealed class AdministrationAccessService
  : IAdministrationAccessService
{
  /// <summary>Gets the bootstrap administration login identifier.</summary>
  public const string BootstrapEmailAddress =
    "townhalladmin@smartcities.local";

  /// <summary>Gets the canonical bootstrap subject accepted only while no whitelist rule exists.</summary>
  public const string BootstrapCanonicalSubject =
    "local-bootstrap:default:townhalladmin@smartcities.local";

  private readonly TownHallContext townHall;
  private readonly IAdministrationAccessRuleStore store;

  /// <summary>Initializes administration admission for one Town Hall deployment.</summary>
  public AdministrationAccessService(
    TownHallContext townHall,
    IAdministrationAccessRuleStore store)
  {
    ArgumentNullException.ThrowIfNull(townHall);
    ArgumentNullException.ThrowIfNull(store);

    this.townHall = townHall;
    this.store = store;
  }

  /// <inheritdoc />
  public string TownHallId =>
    townHall.TownHallId;

  /// <inheritdoc />
  public async Task<bool> IsAuthorizedAsync(
    AdministrationIdentity identity,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(identity);

    var rules = await GetRulesAsync(
        cancellationToken)
      .ConfigureAwait(false);

    if (rules.Count == 0)
    {
      return string.Equals(
        identity.SubjectId,
        BootstrapCanonicalSubject,
        StringComparison.Ordinal);
    }

    return rules.Any(
      rule => Matches(rule, identity));
  }

  /// <inheritdoc />
  public async Task<bool> IsBootstrapAvailableAsync(
    CancellationToken cancellationToken = default) =>
    (await GetRulesAsync(cancellationToken)
      .ConfigureAwait(false)).Count == 0;

  /// <inheritdoc />
  public Task<IReadOnlyList<AdministrationAccessRule>>
    GetRulesAsync(
      CancellationToken cancellationToken = default) =>
    store.GetAllAsync(
      TownHallId,
      cancellationToken);

  /// <inheritdoc />
  public async Task<AdministrationAccessRule> AddRuleAsync(
    AdministrationAccessRuleKind kind,
    string value,
    CancellationToken cancellationToken = default)
  {
    var candidate = AdministrationAccessRule.Create(
      Guid.NewGuid().ToString("N"),
      kind,
      value);
    var existing = (await GetRulesAsync(
        cancellationToken)
      .ConfigureAwait(false))
      .SingleOrDefault(
        rule =>
          rule.Kind == candidate.Kind
          && string.Equals(
            rule.Value,
            candidate.Value,
            RuleValueComparison(candidate.Kind)));

    if (existing is not null)
    {
      return existing;
    }

    await store
      .AddAsync(
        TownHallId,
        candidate,
        cancellationToken)
      .ConfigureAwait(false);

    return candidate;
  }

  /// <inheritdoc />
  public Task<bool> DeleteRuleAsync(
    string ruleId,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);

    return store.DeleteAsync(
      TownHallId,
      ruleId.Trim(),
      cancellationToken);
  }

  private static bool Matches(
    AdministrationAccessRule rule,
    AdministrationIdentity identity) =>
    rule.Kind switch
    {
      AdministrationAccessRuleKind.Email =>
        identity.EmailAddress is not null
        && string.Equals(
          identity.EmailAddress,
          rule.Value,
          StringComparison.OrdinalIgnoreCase),

      AdministrationAccessRuleKind.EmailDomain =>
        identity.EmailAddress is not null
        && string.Equals(
          EmailDomain(identity.EmailAddress),
          rule.Value,
          StringComparison.OrdinalIgnoreCase),

      AdministrationAccessRuleKind.CanonicalSubject =>
        string.Equals(
          identity.SubjectId,
          rule.Value,
          StringComparison.Ordinal),

      _ => false,
    };

  private static string EmailDomain(
    string emailAddress) =>
    emailAddress[
      (emailAddress.LastIndexOf(
        '@') + 1)..];

  private static StringComparison RuleValueComparison(
    AdministrationAccessRuleKind kind) =>
    kind is AdministrationAccessRuleKind.Email
      or AdministrationAccessRuleKind.EmailDomain
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;
}
