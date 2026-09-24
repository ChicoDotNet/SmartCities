using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Resources;

namespace SmartCities.Api.Localization;

/// <summary>
/// Resolves public API localization values from embedded .resx resources.
/// </summary>
public sealed class ResxApiLocalizationCatalog : IApiLocalizationCatalog
{
  /// <summary>Neutral English culture used as the deterministic fallback.</summary>
  public const string NeutralEnglishCulture = "en";

  /// <summary>Mexico Spanish culture supported by the first citizen experience.</summary>
  public const string MexicanSpanishCulture = "es-MX";

  private static readonly CultureInfo NeutralEnglish =
    CultureInfo.GetCultureInfo(NeutralEnglishCulture);

  private static readonly CultureInfo MexicanSpanish =
    CultureInfo.GetCultureInfo(MexicanSpanishCulture);

  private static readonly ResourceManager ResourceManager =
    new(
      "SmartCities.Api.Resources.ApiResources",
      typeof(ResxApiLocalizationCatalog).Assembly);

  private static readonly string[] PublicKeys =
    LoadPublicKeys();

  /// <inheritdoc />
  public ApiLocalizationResourceResponse GetResources(
    string? requestedCulture)
  {
    var requested = string.IsNullOrWhiteSpace(requestedCulture)
      ? NeutralEnglishCulture
      : requestedCulture.Trim();

    var resolved = ResolveCulture(requested);
    var resources = new SortedDictionary<string, string>(
      StringComparer.Ordinal);

    foreach (var key in PublicKeys)
    {
      resources.Add(
        key,
        GetStringForResolvedCulture(key, resolved));
    }

    return new ApiLocalizationResourceResponse(
      requested,
      resolved.Name,
      new ReadOnlyDictionary<string, string>(resources));
  }

  /// <inheritdoc />
  public string GetString(
    string key,
    CultureInfo culture)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(key);
    ArgumentNullException.ThrowIfNull(culture);

    return GetStringForResolvedCulture(
      key,
      ResolveCulture(culture.Name));
  }

  private static string[] LoadPublicKeys()
  {
    var resourceSet = ResourceManager.GetResourceSet(
        CultureInfo.InvariantCulture,
        createIfNotExists: true,
        tryParents: true)
      ?? throw new InvalidOperationException(
        "Neutral API localization resources could not be loaded.");

    return resourceSet
      .Cast<DictionaryEntry>()
      .Select(static entry => entry.Key as string)
      .Where(static key => key is not null)
      .Cast<string>()
      .Order(StringComparer.Ordinal)
      .ToArray();
  }

  private static CultureInfo ResolveCulture(
    string requestedCulture)
  {
    try
    {
      var requested = CultureInfo.GetCultureInfo(
        requestedCulture);

      if (string.Equals(
        requested.Name,
        MexicanSpanishCulture,
        StringComparison.OrdinalIgnoreCase))
      {
        return MexicanSpanish;
      }
    }
    catch (CultureNotFoundException)
    {
      // Unsupported or malformed culture names deterministically use neutral English.
    }

    return NeutralEnglish;
  }

  private static string GetStringForResolvedCulture(
    string key,
    CultureInfo resolvedCulture)
  {
    var resourceCulture = string.Equals(
      resolvedCulture.Name,
      NeutralEnglishCulture,
      StringComparison.Ordinal)
        ? CultureInfo.InvariantCulture
        : resolvedCulture;

    return ResourceManager.GetString(
        key,
        resourceCulture)
      ?? throw new InvalidOperationException(
        $"Required API localization resource '{key}' is missing.");
  }
}
