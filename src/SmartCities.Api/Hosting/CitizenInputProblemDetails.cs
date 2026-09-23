using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Api.Localization;

namespace SmartCities.Api.Hosting;

internal static class CitizenInputProblemDetails
{
  internal const string ProblemType =
    "urn:smartcities:problem:invalid-input";

  internal const string ProblemCode =
    "invalid_input";

  private const string InvalidTitleKey =
    "problem.invalidInput.title";

  private const string InvalidDetailKey =
    "problem.invalidInput.detail";

  private static readonly ResxApiLocalizationCatalog DefaultCatalog =
    new();

  private static readonly CultureInfo NeutralEnglish =
    CultureInfo.GetCultureInfo(
      ResxApiLocalizationCatalog.NeutralEnglishCulture);

  private static readonly HashSet<string> KnownDomainInputParameters =
    new(StringComparer.Ordinal)
    {
      "reportId",
      "caseId",
      "categoryKey",
      "locationReference",
      "description",
      "evidenceReferences",
      "evidenceId",
      "kind",
      "sourceSystem",
      "sourceReference",
      "observedAt",
    };

  internal static bool IsKnownDomainInputParameter(
    string? parameterName) =>
    parameterName is not null
    && KnownDomainInputParameters.Contains(parameterName);

  internal static ProblemDetails Create(
    IEnumerable<string> fields) =>
    Create(
      fields,
      DefaultCatalog,
      NeutralEnglish);

  internal static ProblemDetails Create(
    IEnumerable<string> fields,
    IApiLocalizationCatalog catalog,
    CultureInfo culture)
  {
    ArgumentNullException.ThrowIfNull(fields);
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(culture);

    var normalizedFields = fields
      .Where(static field => !string.IsNullOrWhiteSpace(field))
      .Select(NormalizeFieldPath)
      .Distinct(StringComparer.Ordinal)
      .Order(StringComparer.Ordinal)
      .ToArray();

    var problem = new ProblemDetails
    {
      Type = ProblemType,
      Title = catalog.GetString(
        InvalidTitleKey,
        culture),
      Status = StatusCodes.Status400BadRequest,
      Detail = catalog.GetString(
        InvalidDetailKey,
        culture),
    };

    problem.Extensions["code"] = ProblemCode;
    problem.Extensions["fields"] = normalizedFields;

    return problem;
  }

  private static string NormalizeFieldPath(string field)
  {
    var segments = field.Split(
      '.',
      StringSplitOptions.RemoveEmptyEntries);

    return string.Join(
      '.',
      segments.Select(static segment =>
      {
        if (segment.Length == 0)
        {
          return segment;
        }

        return string.Concat(
          char.ToLowerInvariant(segment[0]),
          segment[1..]);
      }));
  }
}
