using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartCities.Api.Citizens;
using SmartCities.Api.Hosting;
using SmartCities.Api.Localization;
using SmartCities.Evidence;
using Xunit;

namespace SmartCities.Api.Tests.Hosting;

public sealed class CitizenInputProblemDetailsTests
{
  [Fact]
  public void Api_registration_configures_deterministic_problem_details_for_model_validation()
  {
    var services = new ServiceCollection();
    services.AddSmartCitiesApiControllers();

    using var provider = services.BuildServiceProvider();
    var options = provider
      .GetRequiredService<IOptions<ApiBehaviorOptions>>()
      .Value;

    var modelState = new ModelStateDictionary();
    modelState.AddModelError(
      "Description",
      "The Description field is required.");

    var actionContext = CreateActionContext(modelState);
    var result = options.InvalidModelStateResponseFactory(actionContext);

    var response = Assert.IsType<BadRequestObjectResult>(result);
    var problem = Assert.IsType<ProblemDetails>(response.Value);

    Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
    Assert.Equal("urn:smartcities:problem:invalid-input", problem.Type);
    Assert.Equal("Invalid citizen input.", problem.Title);
    Assert.Equal(
      "One or more supplied values are invalid.",
      problem.Detail);
    Assert.Equal("invalid_input", problem.Extensions["code"]);

    var fields = Assert.IsType<string[]>(problem.Extensions["fields"]);
    Assert.Equal(["description"], fields);
  }

  [Fact]
  public void Domain_argument_errors_for_known_input_fields_use_the_same_problem_contract()
  {
    var filter = new InvalidCitizenInputExceptionFilter(
      new ResxApiLocalizationCatalog());
    var context = new ExceptionContext(
      CreateActionContext(new ModelStateDictionary()),
      [])
    {
      Exception = new ArgumentException(
        "Value cannot be empty.",
        "locationReference"),
    };

    filter.OnException(context);

    Assert.True(context.ExceptionHandled);

    var response = Assert.IsType<BadRequestObjectResult>(
      context.Result);
    var problem = Assert.IsType<ProblemDetails>(response.Value);

    Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
    Assert.Equal("urn:smartcities:problem:invalid-input", problem.Type);
    Assert.Equal("invalid_input", problem.Extensions["code"]);
    Assert.Equal(
      ["locationReference"],
      Assert.IsType<string[]>(problem.Extensions["fields"]));
  }

  [Fact]
  public void Unknown_argument_errors_are_not_reclassified_as_citizen_input()
  {
    var filter = new InvalidCitizenInputExceptionFilter(
      new ResxApiLocalizationCatalog());
    var context = new ExceptionContext(
      CreateActionContext(new ModelStateDictionary()),
      [])
    {
      Exception = new ArgumentException(
        "Unexpected internal argument failure.",
        "internalOption"),
    };

    filter.OnException(context);

    Assert.False(context.ExceptionHandled);
    Assert.Null(context.Result);
  }

  [Fact]
  public void Citizen_report_http_contract_marks_primary_constructor_inputs_as_required()
  {
    AssertRequiredParameter<CreateCitizenMobilityReportRequest>(
      nameof(CreateCitizenMobilityReportRequest.ReportId));
    AssertRequiredParameter<CreateCitizenMobilityReportRequest>(
      nameof(CreateCitizenMobilityReportRequest.CaseId));
    AssertRequiredParameter<CreateCitizenMobilityReportRequest>(
      nameof(CreateCitizenMobilityReportRequest.CategoryKey));
    AssertRequiredParameter<CreateCitizenMobilityReportRequest>(
      nameof(CreateCitizenMobilityReportRequest.LocationReference));
    AssertRequiredParameter<CreateCitizenMobilityReportRequest>(
      nameof(CreateCitizenMobilityReportRequest.Description));
  }

  [Fact]
  public void Evidence_http_contract_validates_primary_constructor_provenance_and_evidence_kind()
  {
    AssertRequiredParameter<CitizenEvidenceReferenceRequest>(
      nameof(CitizenEvidenceReferenceRequest.EvidenceId));
    AssertRequiredParameter<CitizenEvidenceReferenceRequest>(
      nameof(CitizenEvidenceReferenceRequest.SourceSystem));
    AssertRequiredParameter<CitizenEvidenceReferenceRequest>(
      nameof(CitizenEvidenceReferenceRequest.SourceReference));

    var kind = GetPrimaryConstructorParameter<CitizenEvidenceReferenceRequest>(
      nameof(CitizenEvidenceReferenceRequest.Kind));

    Assert.NotNull(
      kind.GetCustomAttribute<EnumDataTypeAttribute>());
  }

  private static ActionContext CreateActionContext(
    ModelStateDictionary modelState) =>
    new(
      new DefaultHttpContext(),
      new RouteData(),
      new ActionDescriptor(),
      modelState);

  private static void AssertRequiredParameter<T>(
    string parameterName)
  {
    var parameter = GetPrimaryConstructorParameter<T>(
      parameterName);

    Assert.NotNull(
      parameter.GetCustomAttribute<RequiredAttribute>());
  }

  private static ParameterInfo GetPrimaryConstructorParameter<T>(
    string parameterName)
  {
    var constructor = Assert.Single(
      typeof(T).GetConstructors());

    return Assert.Single(
      constructor.GetParameters(),
      parameter => string.Equals(
        parameter.Name,
        parameterName,
        StringComparison.Ordinal));
  }
}
