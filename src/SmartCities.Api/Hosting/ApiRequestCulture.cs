using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace SmartCities.Api.Hosting;

internal static class ApiRequestCulture
{
  private static readonly CultureInfo NeutralEnglish =
    CultureInfo.GetCultureInfo("en");

  internal static CultureInfo GetUiCulture(
    HttpContext httpContext)
  {
    ArgumentNullException.ThrowIfNull(httpContext);

    return httpContext.Features
        .Get<IRequestCultureFeature>()
        ?.RequestCulture.UICulture
      ?? NeutralEnglish;
  }
}
