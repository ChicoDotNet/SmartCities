using System.ComponentModel.DataAnnotations;
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
  public void Citizen_report_http_contract_marks_required_fields_as_required()
  {
    var request = new CreateCitizenMobilityReportRequest(
      " ",
      " ",
      " ",
      " ",
      " ",
      []);

    var results = new List<ValidationResult>();

    var valid = Validator.TryValidateObject(
      request,
      new ValidationContext(request),
      results,
      validateAllProperties: true);

    Assert.False(valid);
    AssertRequired(results, nameof(CreateCitizenMobilityReportRequest.ReportId));
    AssertRequired(results, nameof(CreateCitizenMobilityReportRequest.CaseId));
    AssertRequired(results, nameof(CreateCitizenMobilityReportRequest.CategoryKey));
    AssertRequired(results, nameof(CreateCitizenMobilityReportRequest.LocationReference));
    AssertRequired(results, nameof(CreateCitizenMobilityReportRequest.Description));
  }

  [Fact]
  public void Evidence_http_contract_validates_required_provenance_and_evidence_kind()
  {
    var evidence = new CitizenEvidenceReferenceRequest(
      " ",
      (EvidenceKind)999,
      " ",
      " ",
      DateTimeOffset.UtcNow);
    var results = new List<ValidationResult>();

    var valid = Validator.TryValidateObject(
      evidence,
      new ValidationContext(evidence),
      results,
      validateAllProperties: true);

    Assert.False(valid);
    AssertRequired(results, nameof(CitizenEvidenceReferenceRequest.EvidenceId));
    AssertRequired(results, nameof(CitizenEvidenceReferenceRequest.SourceSystem));
    AssertRequired(results, nameof(CitizenEvidenceReferenceRequest.SourceReference));
    Assert.Contains(
      results,
      result => result.MemberNames.Contains(
        nameof(CitizenEvidenceReferenceRequest.Kind),
        StringComparer.Ordinal));
  }

  private static ActionContext CreateActionContext(
    ModelStateDictionary modelState) =>
    new(
      new DefaultHttpContext(),
      new RouteData(),
      new ActionDescriptor(),
      modelState);

  private static void AssertRequired(
    IEnumerable<ValidationResult> results,
    string memberName)
  {
    Assert.Contains(
      results,
      result => result.MemberNames.Contains(
        memberName,
        StringComparer.Ordinal));
  }
}
