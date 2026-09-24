namespace SmartCities.Application.Administration;

/// <summary>
/// Defines stable resource-type identifiers used by control-plane audit events.
/// </summary>
public static class AdministrationAuditResourceTypes
{
  /// <summary>One registered feature flag.</summary>
  public const string FeatureFlag = "feature-flag";

  /// <summary>One Administration whitelist rule.</summary>
  public const string WhitelistRule = "administration-whitelist-rule";

  /// <summary>One persisted Administration authorization grant.</summary>
  public const string AuthorizationGrant = "administration-authorization-grant";
}
