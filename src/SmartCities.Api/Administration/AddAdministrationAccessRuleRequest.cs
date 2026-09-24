namespace SmartCities.Api.Administration;

/// <summary>
/// Requests one portable Administration whitelist rule.
/// </summary>
/// <param name="Kind">Rule kind: email-domain, email, or canonical-subject.</param>
/// <param name="Value">Rule value to normalize and persist.</param>
public sealed record AddAdministrationAccessRuleRequest(
  string Kind,
  string Value);
