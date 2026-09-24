using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Application.Administration;
using SmartCities.Identity;

namespace SmartCities.Api.Administration;

/// <summary>
/// Manages the current Town Hall Administration whitelist.
/// </summary>
[ApiController]
[Route("api/administration/whitelist")]
public sealed class AdministrationWhitelistController
  : ControllerBase
{
  private readonly IAdministrationAccessService service;

  /// <summary>Initializes the whitelist management boundary.</summary>
  public AdministrationWhitelistController(
    IAdministrationAccessService service)
  {
    ArgumentNullException.ThrowIfNull(service);
    this.service = service;
  }

  /// <summary>Gets all configured Administration admission rules.</summary>
  [HttpGet]
  [Authorize(
    Policy = SmartCitiesPolicies.ManageAdministrationWhitelist)]
  [ProducesResponseType<
    IReadOnlyList<AdministrationAccessRuleResponse>>(
      StatusCodes.Status200OK)]
  public async Task<
    ActionResult<IReadOnlyList<AdministrationAccessRuleResponse>>>
    GetAsync(
      CancellationToken cancellationToken)
  {
    SetNoStore();

    var rules = await service
      .GetRulesAsync(cancellationToken)
      .ConfigureAwait(false);

    return Ok(
      rules.Select(ToResponse).ToArray());
  }

  /// <summary>Adds or reuses one normalized whitelist rule.</summary>
  [HttpPost]
  [Authorize(
    Policy = SmartCitiesPolicies.ManageAdministrationWhitelist)]
  [ProducesResponseType<AdministrationAccessRuleResponse>(
    StatusCodes.Status200OK)]
  public async Task<ActionResult<AdministrationAccessRuleResponse>>
    AddAsync(
      [FromBody] AddAdministrationAccessRuleRequest request,
      CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);
    SetNoStore();

    var kind = ParseKind(request.Kind);
    var rule = await service
      .AddRuleAsync(
        kind,
        request.Value,
        cancellationToken)
      .ConfigureAwait(false);

    return Ok(ToResponse(rule));
  }

  /// <summary>Deletes one whitelist rule.</summary>
  [HttpDelete("{ruleId}")]
  [Authorize(
    Policy = SmartCitiesPolicies.ManageAdministrationWhitelist)]
  [ProducesResponseType(
    StatusCodes.Status204NoContent)]
  [ProducesResponseType(
    StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteAsync(
    string ruleId,
    CancellationToken cancellationToken)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
    SetNoStore();

    return await service
      .DeleteRuleAsync(
        ruleId,
        cancellationToken)
      .ConfigureAwait(false)
        ? NoContent()
        : NotFound();
  }

  private static AdministrationAccessRuleKind ParseKind(
    string kind)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(kind);

    return kind.Trim().ToLowerInvariant() switch
    {
      "email" =>
        AdministrationAccessRuleKind.Email,
      "email-domain" =>
        AdministrationAccessRuleKind.EmailDomain,
      "canonical-subject" =>
        AdministrationAccessRuleKind.CanonicalSubject,
      _ => throw new ArgumentException(
        "Unsupported Administration whitelist rule kind.",
        nameof(kind)),
    };
  }

  private static AdministrationAccessRuleResponse ToResponse(
    AdministrationAccessRule rule) =>
    new(
      rule.RuleId,
      rule.Kind switch
      {
        AdministrationAccessRuleKind.Email =>
          "email",
        AdministrationAccessRuleKind.EmailDomain =>
          "email-domain",
        AdministrationAccessRuleKind.CanonicalSubject =>
          "canonical-subject",
        _ => throw new InvalidOperationException(
          "Unsupported Administration whitelist rule kind."),
      },
      rule.Value);

  private void SetNoStore()
  {
    Response.Headers.CacheControl = "no-store";
    Response.Headers.Pragma = "no-cache";
  }
}
