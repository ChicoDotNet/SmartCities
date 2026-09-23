using Microsoft.AspNetCore.Mvc;

namespace SmartCities.Api.Hosting;

internal static class CitizenInputProblemDetails
{
  internal const string ProblemType =
    "urn:smartcities:problem:invalid-input";

  internal const string ProblemCode =
    "invalid_input";

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
    IEnumerable<string> fields)
  {
    ArgumentNullException.ThrowIfNull(fields);

    var normalizedFields = fields
      .Where(static field => !string.IsNullOrWhiteSpace(field))
      .Select(NormalizeFieldPath)
      .Distinct(StringComparer.Ordinal)
      .Order(StringComparer.Ordinal)
      .ToArray();

    var problem = new ProblemDetails
    {
      Type = ProblemType,
      Title = "Invalid citizen input.",
      Status = StatusCodes.Status400BadRequest,
      Detail = "One or more supplied values are invalid.",
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
          segment.AsSpan(1));
      }));
  }
}
