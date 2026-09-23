using SmartCities.Decisions;
using Xunit;

namespace SmartCities.Core.Tests.Decisions;

public sealed class DecisionReviewTests
{
    [Fact]
    public void Pending_review_has_no_authority_or_final_disposition()
    {
        var review = DecisionReview.Pending("recommendation-001");

        Assert.Equal(DecisionReviewStatus.PendingHumanReview, review.Status);
        Assert.Null(review.Authority);
        Assert.Null(review.Disposition);
    }

    [Fact]
    public void Finalize_records_the_human_authority_and_disposition()
    {
        var review = DecisionReview.Pending("recommendation-001");
        var authority = HumanAuthority.Create("reviewer-001", "Mobility authority");

        var finalized = review.Finalize(authority, DecisionDisposition.Accepted);

        Assert.Equal(DecisionReviewStatus.Finalized, finalized.Status);
        Assert.Equal(authority, finalized.Authority);
        Assert.Equal(DecisionDisposition.Accepted, finalized.Disposition);
        Assert.Equal("recommendation-001", finalized.RecommendationId);
    }

    [Fact]
    public void Finalize_requires_a_human_authority()
    {
        var review = DecisionReview.Pending("recommendation-001");

        Assert.Throws<ArgumentNullException>(
            () => review.Finalize(null!, DecisionDisposition.Accepted));
    }

    [Fact]
    public void Finalized_review_cannot_be_finalized_again()
    {
        var authority = HumanAuthority.Create("reviewer-001", "Mobility authority");
        var review = DecisionReview.Pending("recommendation-001")
            .Finalize(authority, DecisionDisposition.Accepted);

        Assert.Throws<InvalidOperationException>(
            () => review.Finalize(authority, DecisionDisposition.Rejected));
    }
}
