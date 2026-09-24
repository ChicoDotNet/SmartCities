using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartCities.Criterion;
using SmartCities.Decisions;
using SmartCities.Infrastructure.Persistence;
using SmartCities.Infrastructure.Persistence.HumanOversight;
using Xunit;

namespace SmartCities.Infrastructure.Tests.HumanOversight;

public sealed class DecisionReviewOutcomeLookupTests
{
  [Fact]
  public async Task Repository_recovers_the_authoritative_review_by_evidence_case()
  {
    await using var connection =
      new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync(
      TestContext.Current.CancellationToken);

    var options =
      new DbContextOptionsBuilder<SmartCitiesDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context =
      new SmartCitiesDbContext(options);
    await context.Database.EnsureCreatedAsync(
      TestContext.Current.CancellationToken);

    var repository =
      new EfDecisionReviewRepository(context);
    var pending = DecisionReview.Pending(
      CriterionDecisionTrace.Create(
        requestId: "request-outcome",
        recommendationId: "recommendation-outcome",
        recommendation:
          CriterionRecommendation.RequiresHumanReview,
        requiresHumanReview: true,
        evidenceReferenceIds: ["evidence-001"],
        publicExplanation: "Public-safe explanation.",
        evidenceCaseId: "case-outcome"));

    await repository.AddPendingAsync(
      pending,
      TestContext.Current.CancellationToken);

    var recovered =
      await repository.GetByEvidenceCaseIdAsync(
        "case-outcome",
        TestContext.Current.CancellationToken);

    Assert.NotNull(recovered);
    Assert.Equal(
      "recommendation-outcome",
      recovered.RecommendationId);
    Assert.Equal(
      "case-outcome",
      recovered.EvidenceCaseId);
  }

  [Fact]
  public async Task Lookup_by_case_fails_closed_when_persistence_contains_more_than_one_review()
  {
    await using var connection =
      new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync(
      TestContext.Current.CancellationToken);

    var options =
      new DbContextOptionsBuilder<SmartCitiesDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context =
      new SmartCitiesDbContext(options);
    await context.Database.EnsureCreatedAsync(
      TestContext.Current.CancellationToken);

    var repository =
      new EfDecisionReviewRepository(context);

    foreach (var recommendationId in new[]
      {
        "recommendation-001",
        "recommendation-002",
      })
    {
      await repository.AddPendingAsync(
        DecisionReview.Pending(
          CriterionDecisionTrace.Create(
            requestId: $"request:{recommendationId}",
            recommendationId: recommendationId,
            recommendation:
              CriterionRecommendation.RequiresHumanReview,
            requiresHumanReview: true,
            evidenceReferenceIds: [],
            publicExplanation: "Public-safe explanation.",
            evidenceCaseId: "case-duplicated")),
        TestContext.Current.CancellationToken);
    }

    await Assert.ThrowsAsync<InvalidOperationException>(
      () => repository.GetByEvidenceCaseIdAsync(
        "case-duplicated",
        TestContext.Current.CancellationToken));
  }
}
