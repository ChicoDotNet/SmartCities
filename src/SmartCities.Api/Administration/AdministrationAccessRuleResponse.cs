namespace SmartCities.Api.Administration;

/// <summary>
/// Represents one Administration whitelist rule.
/// </summary>
/// <param name="RuleId">Stable rule identifier.</param>
/// <param name="Kind">Stable machine rule kind.</param>
/// <param name="Value">Normalized rule value.</param>
public sealed record AdministrationAccessRuleResponse(
  string RuleId,
  string Kind,
  string Value);
