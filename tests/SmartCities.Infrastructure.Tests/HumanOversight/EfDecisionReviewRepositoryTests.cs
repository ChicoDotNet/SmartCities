using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartCities.Application.HumanOversight;
using SmartCities.Criterion;
using SmartCities.Decisions;
using SmartCities.Infrastructure.Persistence;
using SmartCities.Infrastructure.Persistence.HumanOversight;
using Xunit;

namespace SmartCities.Infrastructure.Tests.HumanOversight;

public sealed class EfDecisionReviewRepositoryTests
{
  [Fact]
  public async Task Repository_persists_trace_and_atomically_finalizes_the_review()
  {
    await using var connection = new SqliteConnection(
      "Data Source=:memory:");
    await connection.OpenAsync(
      TestContext.Current.CancellationToken);

    var options =
      new DbContextOptionsBuilder<SmartCitiesDbContext>()
        .UseSqlite(connection)
        .Options;

    await using (var setup =
      new SmartCitiesDbContext(options))
    {
      await setup.Database.EnsureCreatedAsync(
        TestContext.Current.CancellationToken);
    }

    var pending = DecisionReview.Pending(
      CriterionDecisionTrace.Create(
        requestId: "request-001",
        recommendationId: "recommendation-001",
        recommendation:
          CriterionRecommendation.RequiresHumanReview,
        requiresHumanReview: true,
        evidenceReferenceIds:
        [
          "evidence-002",
          "evidence-001",
        ],
        publicExplanation: "Public-safe explanation.",
        evidenceCaseId: "case-001"));

    await using (var first =
      new SmartCitiesDbContext(options))
    {
      var repository =
        new EfDecisionReviewRepository(first);

      await repository.AddPendingAsync(
        pending,
        TestContext.Current.CancellationToken);
    }

    var authority = HumanAuthority.Create(
      "reviewer-001",
      "mobility-reviewer");

    await using (var finalizeContext =
      new SmartCitiesDbContext(options))
    {
      var repository =
        new EfDecisionReviewRepository(
          finalizeContext);

      var result = await repository.FinalizeAsync(
        "recommendation-001",
        authority,
        DecisionDisposition.Modified,
        TestContext.Current.CancellationToken);

      Assert.Equal(
        DecisionReviewFinalizationOutcome.Finalized,
        result.Outcome);
    }

    await using (var fresh =
      new SmartCitiesDbContext(options))
    {
      var repository =
        new EfDecisionReviewRepository(fresh);
      var persisted = await repository.GetAsync(
        "recommendation-001",
        TestContext.Current.CancellationToken);

      Assert.NotNull(persisted);
      Assert.Equal(
        "request-001",
        persisted.CriterionRequestId);
      Assert.Equal(
        "case-001",
        persisted.EvidenceCaseId);
      Assert.Equal(
        ["evidence-002", "evidence-001"],
        persisted.EvidenceReferenceIds);
      Assert.Equal(
        DecisionReviewStatus.Finalized,
        persisted.Status);
      Assert.Equal(authority, persisted.Authority);
      Assert.Equal(
        DecisionDisposition.Modified,
        persisted.Disposition);
    }
  }

  [Fact]
  public async Task Second_finalization_preserves_the_first_human_authority()
  {
    await using var connection = new SqliteConnection(
      "Data Source=:memory:");
    await connection.OpenAsync(
      TestContext.Current.CancellationToken);

    var options =
      new DbContextOptionsBuilder<SmartCitiesDbContext>()
        .UseSqlite(connection)
        .Options;

    await using (var setup =
      new SmartCitiesDbContext(options))
    {
      await setup.Database.EnsureCreatedAsync(
        TestContext.Current.CancellationToken);
      var repository =
        new EfDecisionReviewRepository(setup);

      await repository.AddPendingAsync(
        DecisionReview.Pending(
          "recommendation-002"),
        TestContext.Current.CancellationToken);
    }

    var firstAuthority = HumanAuthority.Create(
      "reviewer-001",
      "mobility-reviewer");

    await using (var first =
      new SmartCitiesDbContext(options))
    {
      var repository =
        new EfDecisionReviewRepository(first);

      await repository.FinalizeAsync(
        "recommendation-002",
        firstAuthority,
        DecisionDisposition.Accepted,
        TestContext.Current.CancellationToken);
    }

    await using (var second =
      new SmartCitiesDbContext(options))
    {
      var repository =
        new EfDecisionReviewRepository(second);

      var result = await repository.FinalizeAsync(
        "recommendation-002",
        HumanAuthority.Create(
          "reviewer-002",
          "mobility-reviewer"),
        DecisionDisposition.Rejected,
        TestContext.Current.CancellationToken);

      Assert.Equal(
        DecisionReviewFinalizationOutcome.AlreadyFinalized,
        result.Outcome);
      Assert.Equal(
        "reviewer-001",
        result.Review?.Authority?.SubjectId);
      Assert.Equal(
        DecisionDisposition.Accepted,
        result.Review?.Disposition);
    }
  }
}
