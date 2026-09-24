namespace SmartCities.Application.Administration;

/// <summary>
/// Defines stable machine actions recorded in the Town Hall control-plane audit trail.
/// </summary>
public static class AdministrationAuditActions
{
  /// <summary>Records an explicit feature-flag configuration write.</summary>
  public const string FeatureFlagSet = "feature-flag.set";

  /// <summary>Records creation of a previously absent Administration whitelist rule.</summary>
  public const string WhitelistRuleEnsure = "administration-whitelist.ensure";

  /// <summary>Records deletion of an Administration whitelist rule.</summary>
  public const string WhitelistRuleDelete = "administration-whitelist.delete";

  /// <summary>Records creation of a previously absent persisted authorization grant.</summary>
  public const string AuthorizationGrantEnsure = "administration-grant.ensure";

  /// <summary>Records deletion of a persisted authorization grant.</summary>
  public const string AuthorizationGrantDelete = "administration-grant.delete";
}
