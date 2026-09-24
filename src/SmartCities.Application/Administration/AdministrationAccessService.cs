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
  public Task<AdministrationAccessRule> AddRuleAsync(
    AdministrationAccessRuleKind kind,
    string value,
    CancellationToken cancellationToken = default) =>
    AddRuleCoreAsync(
      kind,
      value,
      auditContext: null,
      cancellationToken);

  /// <inheritdoc />
  public Task<AdministrationAccessRule> AddRuleAsync(
    AdministrationAccessRuleKind kind,
    string value,
    AdministrationControlPlaneAuditContext auditContext,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(auditContext);

    return AddRuleCoreAsync(
      kind,
      value,
      auditContext,
      cancellationToken);
  }

  private async Task<AdministrationAccessRule> AddRuleCoreAsync(
    AdministrationAccessRuleKind kind,
    string value,
    AdministrationControlPlaneAuditContext? auditContext,
    CancellationToken cancellationToken)
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

    if (auditContext is null)
    {
      await store
        .AddAsync(
          TownHallId,
          candidate,
          cancellationToken)
        .ConfigureAwait(false);
    }
    else
    {
      var auditEvent =
        AdministrationControlPlaneAuditEvent.CreateMutation(
          TownHallId,
          auditContext,
          AdministrationAuditActions.WhitelistRuleEnsure,
          AdministrationAuditResourceTypes.WhitelistRule,
          candidate.RuleId,
          $"{RuleKindValue(candidate.Kind)}:{candidate.Value}",
          previousValue: null,
          newValue: "present");

      await store
        .AddAsync(
          TownHallId,
          candidate,
          auditEvent,
          cancellationToken)
        .ConfigureAwait(false);
    }

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

  /// <inheritdoc />
  public async Task<bool> DeleteRuleAsync(
    string ruleId,
    AdministrationControlPlaneAuditContext auditContext,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
    ArgumentNullException.ThrowIfNull(auditContext);

    var normalizedId = ruleId.Trim();
    var existing = (await GetRulesAsync(cancellationToken)
      .ConfigureAwait(false))
      .SingleOrDefault(
        rule => string.Equals(
          rule.RuleId,
          normalizedId,
          StringComparison.Ordinal));

    if (existing is null)
    {
      return false;
    }

    var auditEvent =
      AdministrationControlPlaneAuditEvent.CreateMutation(
        TownHallId,
        auditContext,
        AdministrationAuditActions.WhitelistRuleDelete,
        AdministrationAuditResourceTypes.WhitelistRule,
        existing.RuleId,
        $"{RuleKindValue(existing.Kind)}:{existing.Value}",
        previousValue: "present",
        newValue: null);

    return await store
      .DeleteAsync(
        TownHallId,
        existing.RuleId,
        auditEvent,
        cancellationToken)
      .ConfigureAwait(false);
  }

  private static string RuleKindValue(
    AdministrationAccessRuleKind kind) =>
    kind switch
    {
      AdministrationAccessRuleKind.Email => "email",
      AdministrationAccessRuleKind.EmailDomain => "email-domain",
      AdministrationAccessRuleKind.CanonicalSubject => "canonical-subject",
      _ => throw new ArgumentOutOfRangeException(
        nameof(kind),
        kind,
        "Unsupported Administration whitelist rule kind."),
    };


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
