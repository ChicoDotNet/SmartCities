using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCities.Application.HumanOversight;
using SmartCities.Decisions;

namespace SmartCities.Infrastructure.Persistence.HumanOversight;

/// <summary>
/// Persists and atomically finalizes decision reviews through EF Core.
/// </summary>
public sealed class EfDecisionReviewRepository
  : IDecisionReviewRepository
{
  private static readonly JsonSerializerOptions JsonOptions =
    new(JsonSerializerDefaults.Web);

  private readonly SmartCitiesDbContext dbContext;

  /// <summary>Initializes the repository with the current EF Core Unit of Work.</summary>
  public EfDecisionReviewRepository(
    SmartCitiesDbContext dbContext)
  {
    ArgumentNullException.ThrowIfNull(dbContext);
    this.dbContext = dbContext;
  }

  /// <inheritdoc />
  public async Task AddPendingAsync(
    DecisionReview review,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(review);

    if (review.Status != DecisionReviewStatus.PendingHumanReview)
    {
      throw new ArgumentException(
        "Only pending decision reviews can enter the review repository through this operation.",
        nameof(review));
    }

    dbContext.DecisionReviews.Add(
      ToRecord(review));

    await dbContext.SaveChangesAsync(cancellationToken)
      .ConfigureAwait(false);
  }

  /// <inheritdoc />
  public async Task<DecisionReview?> GetAsync(
    string recommendationId,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(recommendationId);

    var record = await dbContext.DecisionReviews
      .AsNoTracking()
      .SingleOrDefaultAsync(
        item => item.RecommendationId == recommendationId,
        cancellationToken)
      .ConfigureAwait(false);

    return record is null
      ? null
      : ToDomain(record);
  }

  /// <inheritdoc />
  public async Task<DecisionReviewFinalizationResult> FinalizeAsync(
    string recommendationId,
    HumanAuthority authority,
    DecisionDisposition disposition,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(recommendationId);
    ArgumentNullException.ThrowIfNull(authority);

    var affected = await dbContext.DecisionReviews
      .Where(
        item =>
          item.RecommendationId == recommendationId
          && item.Status == DecisionReviewStatus.PendingHumanReview)
      .ExecuteUpdateAsync(
        setters => setters
          .SetProperty(
            item => item.Status,
            DecisionReviewStatus.Finalized)
          .SetProperty(
            item => item.AuthoritySubjectId,
            authority.SubjectId)
          .SetProperty(
            item => item.AuthorityRole,
            authority.Role)
          .SetProperty(
            item => item.Disposition,
            disposition),
        cancellationToken)
      .ConfigureAwait(false);

    var authoritative = await GetAsync(
        recommendationId,
        cancellationToken)
      .ConfigureAwait(false);

    if (authoritative is null)
    {
      return DecisionReviewFinalizationResult.NotFound();
    }

    return affected == 1
      ? DecisionReviewFinalizationResult.Finalized(
          authoritative)
      : DecisionReviewFinalizationResult.AlreadyFinalized(
          authoritative);
  }

  private static DecisionReviewRecord ToRecord(
    DecisionReview review) =>
    new()
    {
      RecommendationId = review.RecommendationId,
      CriterionRequestId = review.CriterionRequestId,
      EvidenceCaseId = review.EvidenceCaseId,
      EvidenceReferenceIdsJson = JsonSerializer.Serialize(
        review.EvidenceReferenceIds,
        JsonOptions),
      Status = review.Status,
      AuthoritySubjectId = review.Authority?.SubjectId,
      AuthorityRole = review.Authority?.Role,
      Disposition = review.Disposition,
    };

  private static DecisionReview ToDomain(
    DecisionReviewRecord record)
  {
    var evidenceReferenceIds =
      JsonSerializer.Deserialize<string[]>(
        record.EvidenceReferenceIdsJson,
        JsonOptions)
      ?? throw new InvalidOperationException(
        "Persisted decision-review evidence identifiers could not be deserialized.");

    HumanAuthority? authority = null;

    if (record.AuthoritySubjectId is not null
      || record.AuthorityRole is not null)
    {
      if (record.AuthoritySubjectId is null
        || record.AuthorityRole is null)
      {
        throw new InvalidOperationException(
          "Persisted decision-review authority is incomplete.");
      }

      authority = HumanAuthority.Create(
        record.AuthoritySubjectId,
        record.AuthorityRole);
    }

    return DecisionReview.Restore(
      record.RecommendationId,
      record.CriterionRequestId,
      record.EvidenceCaseId,
      evidenceReferenceIds,
      record.Status,
      authority,
      record.Disposition);
  }
}
