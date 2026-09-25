namespace SmartCities.CityContext;

/// <summary>
/// Represents one stable, non-localized destination category used by Urban Accessibility.
/// </summary>
/// <remarks>
/// User-facing labels belong to localization resources. The category identifier is canonical product data.
/// </remarks>
public sealed record DestinationCategory
{
  private DestinationCategory(
    string categoryId)
  {
    CategoryId = categoryId;
  }

  /// <summary>Gets the stable non-localized category identifier.</summary>
  public string CategoryId { get; }

  /// <summary>Creates a destination category.</summary>
  public static DestinationCategory Create(
    string categoryId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(
      categoryId);

    return new DestinationCategory(
      categoryId.Trim());
  }
}
