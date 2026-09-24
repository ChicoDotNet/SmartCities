using Microsoft.EntityFrameworkCore;
using SmartCities.Infrastructure.Persistence.Citizens;
using SmartCities.Infrastructure.Persistence.HumanOversight;

namespace SmartCities.Infrastructure.Persistence;

/// <summary>
/// EF Core persistence session for SmartCities application repositories.
/// </summary>
/// <remarks>
/// A scoped instance is the Unit of Work boundary for repository operations. Provider selection and migrations
/// are configured by the composition root rather than by this type.
/// </remarks>
public sealed class SmartCitiesDbContext : DbContext
{
  /// <summary>Initializes the persistence session with externally configured EF Core options.</summary>
  /// <param name="options">Provider and connection options supplied by the composition root.</param>
  public SmartCitiesDbContext(
    DbContextOptions<SmartCitiesDbContext> options)
    : base(options)
  {
  }

  internal DbSet<CitizenMobilityReportRecord> CitizenMobilityReports =>
    Set<CitizenMobilityReportRecord>();

  internal DbSet<DecisionReviewRecord> DecisionReviews =>
    Set<DecisionReviewRecord>();

  /// <inheritdoc />
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    var report = modelBuilder.Entity<CitizenMobilityReportRecord>();
    report.ToTable("CitizenMobilityReports");
    report.HasKey(static entity => entity.ReportId);
    report.Property(static entity => entity.ReportId).HasMaxLength(128);
    report.Property(static entity => entity.CaseId).HasMaxLength(128);
    report.Property(static entity => entity.Subject).HasMaxLength(2048);
    report.HasIndex(static entity => entity.CaseId).IsUnique();

    report
      .HasMany(static entity => entity.EvidenceReferences)
      .WithOne(static entity => entity.Report)
      .HasForeignKey(static entity => entity.ReportId)
      .OnDelete(DeleteBehavior.Cascade);

    var evidence = modelBuilder.Entity<EvidenceReferenceRecord>();
    evidence.ToTable("CitizenMobilityEvidenceReferences");
    evidence.HasKey(static entity => new
    {
      entity.ReportId,
      entity.EvidenceId,
    });

    evidence.Property(static entity => entity.ReportId).HasMaxLength(128);
    evidence.Property(static entity => entity.EvidenceId).HasMaxLength(128);
    evidence.Property(static entity => entity.SourceSystem).HasMaxLength(128);
    evidence.Property(static entity => entity.SourceReference).HasMaxLength(512);

    evidence
      .HasIndex(static entity => new
      {
        entity.ReportId,
        entity.Position,
      })
      .IsUnique();

    var review = modelBuilder.Entity<DecisionReviewRecord>();
    review.ToTable("DecisionReviews");
    review.HasKey(static entity => entity.RecommendationId);
    review.Property(static entity => entity.RecommendationId)
      .HasMaxLength(128);
    review.Property(static entity => entity.CriterionRequestId)
      .HasMaxLength(128);
    review.Property(static entity => entity.EvidenceCaseId)
      .HasMaxLength(128);
    review.Property(static entity => entity.AuthoritySubjectId)
      .HasMaxLength(256);
    review.Property(static entity => entity.AuthorityRole)
      .HasMaxLength(128);
  }
}
