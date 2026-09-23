using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartCities.Api.Localization;

namespace SmartCities.Api.Hosting;

/// <summary>
/// Converts known domain input argument failures into the stable citizen-input HTTP problem contract.
/// </summary>
/// <remarks>
/// Unknown argument failures are deliberately left unhandled so programming or infrastructure errors are not
/// mislabeled as citizen mistakes.
/// </remarks>
public sealed class InvalidCitizenInputExceptionFilter : IExceptionFilter
{
  private readonly IApiLocalizationCatalog catalog;

  /// <summary>Initializes a filter with the configured API localization catalog.</summary>
  /// <param name="catalog">API localization catalog.</param>
  public InvalidCitizenInputExceptionFilter(
    IApiLocalizationCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    this.catalog = catalog;
  }

  /// <inheritdoc />
  public void OnException(ExceptionContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    if (context.Exception is not ArgumentException argumentException
      || !CitizenInputProblemDetails.IsKnownDomainInputParameter(
        argumentException.ParamName))
    {
      return;
    }

    context.Result = new BadRequestObjectResult(
      CitizenInputProblemDetails.Create(
        [argumentException.ParamName!],
        catalog,
        ApiRequestCulture.GetUiCulture(
          context.HttpContext)));

    context.ExceptionHandled = true;
  }
}
