using SmartCities.Application.HumanOversight;
using SmartCities.Decisions;
using Xunit;

namespace SmartCities.Application.Tests.HumanOversight;

public sealed class DecisionReviewServiceTests
{
  [Fact]
  public async Task Service_finalizes_a_pending_review_under_the_supplied_human_authority()
  {
    var repository = new RecordingDecisionReviewRepository(
      DecisionReview.Pending("recommendation-001"));
    var service = new DecisionReviewService(repository);
    var authority = HumanAuthority.Create(
      "reviewer-001",
      "mobility-reviewer");

    var result = await service.FinalizeAsync(
      "recommendation-001",
      authority,
      DecisionDisposition.Accepted,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      DecisionReviewFinalizationOutcome.Finalized,
      result.Outcome);
    Assert.NotNull(result.Review);
    Assert.Equal(authority, result.Review.Authority);
    Assert.Equal(
      DecisionDisposition.Accepted,
      result.Review.Disposition);
  }

  [Fact]
  public async Task Service_preserves_not_found_and_already_finalized_outcomes()
  {
    var repository = new RecordingDecisionReviewRepository(
      review: null);
    var service = new DecisionReviewService(repository);
    var authority = HumanAuthority.Create(
      "reviewer-001",
      "mobility-reviewer");

    var missing = await service.FinalizeAsync(
      "missing",
      authority,
      DecisionDisposition.Rejected,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      DecisionReviewFinalizationOutcome.NotFound,
      missing.Outcome);

    repository.Review = DecisionReview
      .Pending("recommendation-002")
      .Finalize(
        authority,
        DecisionDisposition.Modified);

    var existing = await service.FinalizeAsync(
      "recommendation-002",
      HumanAuthority.Create(
        "reviewer-002",
        "mobility-reviewer"),
      DecisionDisposition.Accepted,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      DecisionReviewFinalizationOutcome.AlreadyFinalized,
      existing.Outcome);
    Assert.Equal(
      "reviewer-001",
      existing.Review?.Authority?.SubjectId);
  }

  private sealed class RecordingDecisionReviewRepository
    : IDecisionReviewRepository
  {
    public RecordingDecisionReviewRepository(
      DecisionReview? review)
    {
      Review = review;
    }

    public DecisionReview? Review { get; set; }

    public Task AddPendingAsync(
      DecisionReview review,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      Review = review;
      return Task.CompletedTask;
    }

    public Task<DecisionReview> GetOrAddPendingAsync(
      DecisionReview review,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (Review is null)
      {
        Review = review;
      }

      return Task.FromResult(Review);
    }

    public Task<DecisionReview?> GetAsync(
      string recommendationId,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      return Task.FromResult(
        Review is not null
        && string.Equals(
          Review.RecommendationId,
          recommendationId,
          StringComparison.Ordinal)
          ? Review
          : null);
    }

    public Task<DecisionReviewFinalizationResult> FinalizeAsync(
      string recommendationId,
      HumanAuthority authority,
      DecisionDisposition disposition,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (Review is null
        || !string.Equals(
          Review.RecommendationId,
          recommendationId,
          StringComparison.Ordinal))
      {
        return Task.FromResult(
          DecisionReviewFinalizationResult.NotFound());
      }

      if (Review.Status == DecisionReviewStatus.Finalized)
      {
        return Task.FromResult(
          DecisionReviewFinalizationResult.AlreadyFinalized(
            Review));
      }

      Review = Review.Finalize(
        authority,
        disposition);

      return Task.FromResult(
        DecisionReviewFinalizationResult.Finalized(
          Review));
    }
  }
}
