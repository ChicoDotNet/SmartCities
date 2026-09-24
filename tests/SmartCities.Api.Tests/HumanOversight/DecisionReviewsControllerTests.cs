using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCities.Api.HumanOversight;
using SmartCities.Application.HumanOversight;
using SmartCities.Decisions;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Api.Tests.HumanOversight;

public sealed class DecisionReviewsControllerTests
{
  [Fact]
  public void Finalize_endpoint_requires_the_canonical_finalize_policy()
  {
    var method = typeof(DecisionReviewsController)
      .GetMethod(
        nameof(
          DecisionReviewsController.FinalizeAsync));

    Assert.NotNull(method);

    var authorize = Assert.Single(
      method.GetCustomAttributes<
        AuthorizeAttribute>());

    Assert.Equal(
      SmartCitiesPolicies.FinalizeDecisionReview,
      authorize.Policy);
  }

  [Fact]
  public async Task Finalize_maps_the_selected_canonical_role_to_human_authority()
  {
    var service = new RecordingService(
      DecisionReviewFinalizationResult.Finalized(
        DecisionReview
          .Pending("recommendation-001")
          .Finalize(
            HumanAuthority.Create(
              "canonical-subject",
              "mobility-reviewer"),
            DecisionDisposition.Accepted)));
    var controller =
      new DecisionReviewsController(service)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext
          {
            User = CreatePrincipal(),
          },
        },
      };

    var result = await controller.FinalizeAsync(
      "recommendation-001",
      new FinalizeDecisionReviewRequest(
        "mobility-reviewer",
        DecisionDisposition.Accepted),
      TestContext.Current.CancellationToken);

    var response = Assert.IsType<OkObjectResult>(
      result.Result);
    var payload =
      Assert.IsType<DecisionReviewResponse>(
        response.Value);

    Assert.Equal(
      "canonical-subject",
      service.LastAuthority?.SubjectId);
    Assert.Equal(
      "mobility-reviewer",
      service.LastAuthority?.Role);
    Assert.Equal(
      DecisionReviewStatus.Finalized,
      payload.Status);
  }

  [Fact]
  public async Task Finalize_returns_404_for_unknown_review()
  {
    var controller = CreateController(
      DecisionReviewFinalizationResult.NotFound());

    var result = await controller.FinalizeAsync(
      "missing",
      new FinalizeDecisionReviewRequest(
        "mobility-reviewer",
        DecisionDisposition.Accepted),
      TestContext.Current.CancellationToken);

    Assert.IsType<NotFoundResult>(
      result.Result);
  }

  [Fact]
  public async Task Finalize_returns_409_when_the_review_is_already_final()
  {
    var finalized = DecisionReview
      .Pending("recommendation-002")
      .Finalize(
        HumanAuthority.Create(
          "first-reviewer",
          "mobility-reviewer"),
        DecisionDisposition.Rejected);
    var controller = CreateController(
      DecisionReviewFinalizationResult
        .AlreadyFinalized(finalized));

    var result = await controller.FinalizeAsync(
      "recommendation-002",
      new FinalizeDecisionReviewRequest(
        "mobility-reviewer",
        DecisionDisposition.Accepted),
      TestContext.Current.CancellationToken);

    var response = Assert.IsType<ConflictObjectResult>(
      result.Result);
    var payload =
      Assert.IsType<DecisionReviewResponse>(
        response.Value);

    Assert.Equal(
      "first-reviewer",
      payload.AuthoritySubjectId);
    Assert.Equal(
      DecisionDisposition.Rejected,
      payload.Disposition);
  }

  private static DecisionReviewsController CreateController(
    DecisionReviewFinalizationResult result)
  {
    var controller =
      new DecisionReviewsController(
        new RecordingService(result));

    controller.ControllerContext =
      new ControllerContext
      {
        HttpContext = new DefaultHttpContext
        {
          User = CreatePrincipal(),
        },
      };

    return controller;
  }

  private static ClaimsPrincipal CreatePrincipal() =>
    new(
      new ClaimsIdentity(
        [
          new Claim(
            SmartCitiesClaimTypes.Subject,
            "canonical-subject"),
          new Claim(
            SmartCitiesClaimTypes.IdentityProvider,
            "test-provider"),
          new Claim(
            SmartCitiesClaimTypes.AuthorityRole,
            "mobility-reviewer"),
          new Claim(
            SmartCitiesClaimTypes.AuthorityRole,
            "auditor"),
          new Claim(
            SmartCitiesClaimTypes.Permission,
            SmartCitiesPermissions
              .FinalizeDecisionReview),
        ],
        authenticationType: "test"));

  private sealed class RecordingService
    : IDecisionReviewService
  {
    private readonly DecisionReviewFinalizationResult result;

    public RecordingService(
      DecisionReviewFinalizationResult result)
    {
      this.result = result;
    }

    public HumanAuthority? LastAuthority { get; private set; }

    public Task<DecisionReviewFinalizationResult> FinalizeAsync(
      string recommendationId,
      HumanAuthority authority,
      DecisionDisposition disposition,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      LastAuthority = authority;

      return Task.FromResult(result);
    }

    public Task<DecisionReview?> GetAsync(
      string recommendationId,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.FromResult(result.Review);
    }
  }
}
