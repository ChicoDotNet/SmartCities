using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Application.Administration;
using SmartCities.Identity;

namespace SmartCities.Api.Administration;

/// <summary>
/// Manages persisted per-Town-Hall canonical authorization grants.
/// </summary>
[ApiController]
[Route("api/administration/grants")]
public sealed class AdministrationAuthorizationGrantsController
  : ControllerBase
{
  private readonly IAdministrationAuthorizationGrantService service;
  private readonly IAdministrationAuthorizationGrantCatalog catalog;

  /// <summary>Initializes the grant-management HTTP boundary.</summary>
  public AdministrationAuthorizationGrantsController(
    IAdministrationAuthorizationGrantService service,
    IAdministrationAuthorizationGrantCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(service);
    ArgumentNullException.ThrowIfNull(catalog);
    this.service = service;
    this.catalog = catalog;
  }

  /// <summary>Gets all persisted grant assignments for the current Town Hall.</summary>
  [HttpGet]
  [Authorize(
    Policy = SmartCitiesPolicies.ManageAdministrationGrants)]
  [ProducesResponseType<IReadOnlyList<AdministrationAuthorizationGrantResponse>>(
    StatusCodes.Status200OK)]
  public async Task<ActionResult<IReadOnlyList<AdministrationAuthorizationGrantResponse>>>
    GetAsync(
      CancellationToken cancellationToken)
  {
    SetNoStore();

    var grants = await service
      .GetAllAsync(cancellationToken)
      .ConfigureAwait(false);

    return Ok(
      grants.Select(ToResponse).ToArray());
  }

  /// <summary>Gets code-owned roles and permissions that may be assigned persistently.</summary>
  [HttpGet("catalog")]
  [Authorize(
    Policy = SmartCitiesPolicies.ManageAdministrationGrants)]
  [ProducesResponseType<AdministrationAuthorizationGrantCatalogResponse>(
    StatusCodes.Status200OK)]
  public ActionResult<AdministrationAuthorizationGrantCatalogResponse>
    GetCatalog()
  {
    SetNoStore();

    return Ok(
      new AdministrationAuthorizationGrantCatalogResponse(
        catalog.AuthorityRoles,
        catalog.Permissions));
  }

  /// <summary>Adds or reuses one normalized persisted grant assignment.</summary>
  [HttpPost]
  [Authorize(
    Policy = SmartCitiesPolicies.ManageAdministrationGrants)]
  [ProducesResponseType<AdministrationAuthorizationGrantResponse>(
    StatusCodes.Status200OK)]
  public async Task<ActionResult<AdministrationAuthorizationGrantResponse>>
    AddAsync(
      [FromBody] AddAdministrationAuthorizationGrantRequest request,
      CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);
    SetNoStore();

    var grant = await service
      .AddAsync(
        ParseTargetKind(request.TargetKind),
        request.TargetValue,
        ParseGrantKind(request.GrantKind),
        request.Value,
        AdministrationAuditContextFactory.Create(
          HttpContext),
        cancellationToken)
      .ConfigureAwait(false);

    return Ok(ToResponse(grant));
  }

  /// <summary>Deletes one persisted grant assignment.</summary>
  [HttpDelete("{grantId}")]
  [Authorize(
    Policy = SmartCitiesPolicies.ManageAdministrationGrants)]
  [ProducesResponseType(
    StatusCodes.Status204NoContent)]
  [ProducesResponseType(
    StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteAsync(
    string grantId,
    CancellationToken cancellationToken)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(grantId);
    SetNoStore();

    return await service
      .DeleteAsync(
        grantId,
        AdministrationAuditContextFactory.Create(
          HttpContext),
        cancellationToken)
      .ConfigureAwait(false)
        ? NoContent()
        : NotFound();
  }

  private static AdministrationAuthorizationGrantResponse ToResponse(
    AdministrationAuthorizationGrant grant) =>
    new(
      grant.GrantId,
      TargetKindValue(grant.TargetKind),
      grant.TargetValue,
      grant.Kind == AdministrationAuthorizationGrantKind.AuthorityRole
        ? "authority-role"
        : "permission",
      grant.Value);

  private static AdministrationAccessRuleKind ParseTargetKind(
    string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    return value.Trim().ToLowerInvariant() switch
    {
      "email-domain" => AdministrationAccessRuleKind.EmailDomain,
      "email" => AdministrationAccessRuleKind.Email,
      "canonical-subject" => AdministrationAccessRuleKind.CanonicalSubject,
      _ => throw new ArgumentException(
        "Unsupported Administration grant target kind.",
        nameof(value)),
    };
  }

  private static AdministrationAuthorizationGrantKind ParseGrantKind(
    string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    return value.Trim().ToLowerInvariant() switch
    {
      "authority-role" => AdministrationAuthorizationGrantKind.AuthorityRole,
      "permission" => AdministrationAuthorizationGrantKind.Permission,
      _ => throw new ArgumentException(
        "Unsupported Administration grant kind.",
        nameof(value)),
    };
  }

  private static string TargetKindValue(
    AdministrationAccessRuleKind kind) =>
    kind switch
    {
      AdministrationAccessRuleKind.EmailDomain => "email-domain",
      AdministrationAccessRuleKind.Email => "email",
      AdministrationAccessRuleKind.CanonicalSubject => "canonical-subject",
      _ => throw new InvalidOperationException(
        "Unsupported Administration grant target kind."),
    };

  private void SetNoStore()
  {
    Response.Headers.CacheControl = "no-store";
    Response.Headers.Pragma = "no-cache";
  }
}
