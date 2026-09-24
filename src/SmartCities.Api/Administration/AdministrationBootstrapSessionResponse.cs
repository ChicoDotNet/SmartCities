namespace SmartCities.Api.Administration;

/// <summary>
/// Describes a successfully established bootstrap Administration browser session.
/// </summary>
/// <param name="SubjectId">Canonical bootstrap subject.</param>
/// <param name="UserName">Bootstrap login identifier.</param>
public sealed record AdministrationBootstrapSessionResponse(
  string SubjectId,
  string UserName);
