using Microsoft.EntityFrameworkCore;
using SmartCities.Infrastructure.Persistence.Citizens;

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

  /// <inheritdoc />
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    var report = modelBuilder.Entity<CitizenMobilityReportRecord>();
    report.HasKey(static entity => entity.ReportId);
    report.HasIndex(static entity => entity.CaseId).IsUnique();

    report
      .HasMany(static entity => entity.EvidenceReferences)
      .WithOne(static entity => entity.Report)
      .HasForeignKey(static entity => entity.ReportId)
      .OnDelete(DeleteBehavior.Cascade);

    var evidence = modelBuilder.Entity<EvidenceReferenceRecord>();
    evidence.HasKey(static entity => new
    {
      entity.ReportId,
      entity.EvidenceId,
    });

    evidence
      .HasIndex(static entity => new
      {
        entity.ReportId,
        entity.Position,
      })
      .IsUnique();
  }
}
