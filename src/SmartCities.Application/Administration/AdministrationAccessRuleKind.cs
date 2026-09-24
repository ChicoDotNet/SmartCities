namespace SmartCities.Application.Administration;

/// <summary>
/// Defines supported portable Town Hall administration admission rule kinds.
/// </summary>
public enum AdministrationAccessRuleKind
{
  /// <summary>Matches one normalized email address exactly.</summary>
  Email = 0,

  /// <summary>Matches the exact domain of a canonical email address.</summary>
  EmailDomain = 1,

  /// <summary>Matches one provider-neutral canonical SmartCities subject exactly.</summary>
  CanonicalSubject = 2,
}
