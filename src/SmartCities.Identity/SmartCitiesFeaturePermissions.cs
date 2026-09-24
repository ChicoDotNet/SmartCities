namespace SmartCities.Identity;

/// <summary>
/// Builds stable feature-specific SmartCities permission identifiers.
/// </summary>
public static class SmartCitiesFeaturePermissions
{
  /// <summary>Builds the operational management permission for one feature.</summary>
  public static string Manage(
    string featureId) =>
    Build(featureId, "manage");

  /// <summary>Builds the deployment configuration permission for one feature.</summary>
  public static string Configure(
    string featureId) =>
    Build(featureId, "config");

  private static string Build(
    string featureId,
    string capability)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(featureId);

    var normalized = featureId.Trim();

    if (normalized.Length > 128
      || normalized[0] == '-'
      || normalized[^1] == '-'
      || normalized.Any(
        static character =>
          !(character is >= 'a' and <= 'z')
          && !char.IsAsciiDigit(character)
          && character != '-'))
    {
      throw new ArgumentException(
        "Feature identifiers must use lowercase ASCII letters, digits, and internal hyphens only.",
        nameof(featureId));
    }

    return $"{normalized}.{capability}";
  }
}
