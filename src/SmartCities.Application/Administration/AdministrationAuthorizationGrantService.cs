using SmartCities.Application.FeatureFlags;

namespace SmartCities.Application.Administration;

/// <summary>
/// Applies code-owned persisted grants only to identities currently admitted by Town Hall Administration.
/// </summary>
public sealed class AdministrationAuthorizationGrantService
  : IAdministrationAuthorizationGrantService
{
  private readonly TownHallContext townHall;
  private readonly IAdministrationAccessService access;
  private readonly IAdministrationAuthorizationGrantStore store;
  private readonly IAdministrationAuthorizationGrantCatalog catalog;

  /// <summary>Initializes persisted Town Hall authorization grant resolution.</summary>
  public AdministrationAuthorizationGrantService(
    TownHallContext townHall,
    IAdministrationAccessService access,
    IAdministrationAuthorizationGrantStore store,
    IAdministrationAuthorizationGrantCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(townHall);
    ArgumentNullException.ThrowIfNull(access);
    ArgumentNullException.ThrowIfNull(store);
    ArgumentNullException.ThrowIfNull(catalog);

    this.townHall = townHall;
    this.access = access;
    this.store = store;
    this.catalog = catalog;
  }

  /// <inheritdoc />
  public string TownHallId =>
    townHall.TownHallId;

  /// <inheritdoc />
  public async Task<AdministrationEffectiveGrants> GetEffectiveAsync(
    AdministrationIdentity identity,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(identity);

    if (!await access
      .IsAuthorizedAsync(identity, cancellationToken)
      .ConfigureAwait(false))
    {
      return AdministrationEffectiveGrants.Empty;
    }

    var grants = await store
      .GetAllAsync(TownHallId, cancellationToken)
      .ConfigureAwait(false);

    return AdministrationEffectiveGrants.Create(
      grants
        .Where(
          grant =>
            grant.Kind == AdministrationAuthorizationGrantKind.AuthorityRole
            && Matches(grant, identity))
        .Select(static grant => grant.Value),
      grants
        .Where(
          grant =>
            grant.Kind == AdministrationAuthorizationGrantKind.Permission
            && Matches(grant, identity))
        .Select(static grant => grant.Value));
  }

  /// <inheritdoc />
  public Task<IReadOnlyList<AdministrationAuthorizationGrant>>
    GetAllAsync(
      CancellationToken cancellationToken = default) =>
    store.GetAllAsync(
      TownHallId,
      cancellationToken);

  /// <inheritdoc />
  public Task<AdministrationAuthorizationGrant> AddAsync(
    AdministrationAccessRuleKind targetKind,
    string targetValue,
    AdministrationAuthorizationGrantKind kind,
    string value,
    CancellationToken cancellationToken = default) =>
    AddCoreAsync(
      targetKind,
      targetValue,
      kind,
      value,
      auditContext: null,
      cancellationToken);

  /// <inheritdoc />
  public Task<AdministrationAuthorizationGrant> AddAsync(
    AdministrationAccessRuleKind targetKind,
    string targetValue,
    AdministrationAuthorizationGrantKind kind,
    string value,
    AdministrationControlPlaneAuditContext auditContext,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(auditContext);

    return AddCoreAsync(
      targetKind,
      targetValue,
      kind,
      value,
      auditContext,
      cancellationToken);
  }

  private async Task<AdministrationAuthorizationGrant> AddCoreAsync(
    AdministrationAccessRuleKind targetKind,
    string targetValue,
    AdministrationAuthorizationGrantKind kind,
    string value,
    AdministrationControlPlaneAuditContext? auditContext,
    CancellationToken cancellationToken)
  {
    ValidateCatalogValue(kind, value);

    var candidate = AdministrationAuthorizationGrant.Create(
      Guid.NewGuid().ToString("N"),
      targetKind,
      targetValue,
      kind,
      value);

    var existing = (await GetAllAsync(cancellationToken)
      .ConfigureAwait(false))
      .SingleOrDefault(
        grant =>
          grant.TargetKind == candidate.TargetKind
          && string.Equals(
            grant.TargetValue,
            candidate.TargetValue,
            TargetComparison(candidate.TargetKind))
          && grant.Kind == candidate.Kind
          && string.Equals(
            grant.Value,
            candidate.Value,
            StringComparison.Ordinal));

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
          AdministrationAuditActions.AuthorizationGrantEnsure,
          AdministrationAuditResourceTypes.AuthorizationGrant,
          candidate.GrantId,
          $"{TargetKindValue(candidate.TargetKind)}:{candidate.TargetValue}|{GrantKindValue(candidate.Kind)}:{candidate.Value}",
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
  public Task<bool> DeleteAsync(
    string grantId,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(grantId);

    return store.DeleteAsync(
      TownHallId,
      grantId.Trim(),
      cancellationToken);
  }

  /// <inheritdoc />
  public async Task<bool> DeleteAsync(
    string grantId,
    AdministrationControlPlaneAuditContext auditContext,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(grantId);
    ArgumentNullException.ThrowIfNull(auditContext);

    var normalizedId = grantId.Trim();
    var existing = (await GetAllAsync(cancellationToken)
      .ConfigureAwait(false))
      .SingleOrDefault(
        grant => string.Equals(
          grant.GrantId,
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
        AdministrationAuditActions.AuthorizationGrantDelete,
        AdministrationAuditResourceTypes.AuthorizationGrant,
        existing.GrantId,
        $"{TargetKindValue(existing.TargetKind)}:{existing.TargetValue}|{GrantKindValue(existing.Kind)}:{existing.Value}",
        previousValue: "present",
        newValue: null);

    return await store
      .DeleteAsync(
        TownHallId,
        existing.GrantId,
        auditEvent,
        cancellationToken)
      .ConfigureAwait(false);
  }

  private static string TargetKindValue(
    AdministrationAccessRuleKind kind) =>
    kind switch
    {
      AdministrationAccessRuleKind.EmailDomain => "email-domain",
      AdministrationAccessRuleKind.Email => "email",
      AdministrationAccessRuleKind.CanonicalSubject => "canonical-subject",
      _ => throw new ArgumentOutOfRangeException(
        nameof(kind),
        kind,
        "Unsupported Administration grant target kind."),
    };

  private static string GrantKindValue(
    AdministrationAuthorizationGrantKind kind) =>
    kind switch
    {
      AdministrationAuthorizationGrantKind.AuthorityRole => "authority-role",
      AdministrationAuthorizationGrantKind.Permission => "permission",
      _ => throw new ArgumentOutOfRangeException(
        nameof(kind),
        kind,
        "Unsupported Administration authorization grant kind."),
    };


  private void ValidateCatalogValue(
    AdministrationAuthorizationGrantKind kind,
    string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    var normalized = value.Trim();
    var allowed = kind switch
    {
      AdministrationAuthorizationGrantKind.AuthorityRole =>
        catalog.AuthorityRoles,
      AdministrationAuthorizationGrantKind.Permission =>
        catalog.Permissions,
      _ => throw new ArgumentOutOfRangeException(
        nameof(kind),
        kind,
        "Unsupported Administration authorization grant kind."),
    };

    if (!allowed.Contains(
        normalized,
        StringComparer.Ordinal))
    {
      throw new ArgumentException(
        $"'{normalized}' is not a code-owned assignable {kind}.",
        nameof(value));
    }
  }

  private static bool Matches(
    AdministrationAuthorizationGrant grant,
    AdministrationIdentity identity) =>
    grant.TargetKind switch
    {
      AdministrationAccessRuleKind.Email =>
        identity.EmailAddress is not null
        && string.Equals(
          identity.EmailAddress,
          grant.TargetValue,
          StringComparison.OrdinalIgnoreCase),

      AdministrationAccessRuleKind.EmailDomain =>
        identity.EmailAddress is not null
        && string.Equals(
          EmailDomain(identity.EmailAddress),
          grant.TargetValue,
          StringComparison.OrdinalIgnoreCase),

      AdministrationAccessRuleKind.CanonicalSubject =>
        string.Equals(
          identity.SubjectId,
          grant.TargetValue,
          StringComparison.Ordinal),

      _ => false,
    };

  private static string EmailDomain(
    string emailAddress) =>
    emailAddress[
      (emailAddress.LastIndexOf('@') + 1)..];

  private static StringComparison TargetComparison(
    AdministrationAccessRuleKind kind) =>
    kind is AdministrationAccessRuleKind.Email
      or AdministrationAccessRuleKind.EmailDomain
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;
}
