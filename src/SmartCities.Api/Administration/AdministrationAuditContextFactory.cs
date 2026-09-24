using SmartCities.Api.Hosting;
using SmartCities.Application.Administration;
using SmartCities.Identity;

namespace SmartCities.Api.Administration;

internal static class AdministrationAuditContextFactory
{
  internal static AdministrationControlPlaneAuditContext Create(
    HttpContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var identity = context.User.ToCanonicalIdentity();
    var correlationId =
      context.Response.Headers[
        SmartCitiesRequestObservabilityMiddleware
          .CorrelationHeaderName]
        .ToString();

    if (string.IsNullOrWhiteSpace(correlationId))
    {
      correlationId = context.TraceIdentifier;
    }

    return AdministrationControlPlaneAuditContext.Create(
      identity.SubjectId,
      identity.IdentityProvider,
      correlationId);
  }
}
