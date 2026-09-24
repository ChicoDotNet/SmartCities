using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartCities.Infrastructure.Persistence;

#nullable disable

namespace SmartCities.Infrastructure.SqlServer.Migrations;

/// <summary>
/// SQL Server model snapshot for the provider-specific migration chain.
/// </summary>
[DbContext(typeof(SmartCitiesDbContext))]
public sealed class SmartCitiesDbContextModelSnapshot : ModelSnapshot
{
  /// <inheritdoc />
  protected override void BuildModel(ModelBuilder modelBuilder)
  {
    modelBuilder
      .HasAnnotation("ProductVersion", "10.0.12")
      .HasAnnotation("Relational:MaxIdentifierLength", 128);

    modelBuilder.Entity(
      "SmartCities.Infrastructure.Persistence.Citizens.CitizenMobilityReportRecord",
      entity =>
      {
        entity.Property<string>("ReportId")
          .HasMaxLength(128)
          .HasColumnType("nvarchar(128)");

        entity.Property<string>("CaseId")
          .IsRequired()
          .HasMaxLength(128)
          .HasColumnType("nvarchar(128)");

        entity.Property<string>("Subject")
          .IsRequired()
          .HasMaxLength(2048)
          .HasColumnType("nvarchar(2048)");

        entity.HasKey("ReportId");

        entity.HasIndex("CaseId")
          .IsUnique();

        entity.ToTable("CitizenMobilityReports");
      });

    modelBuilder.Entity(
      "SmartCities.Infrastructure.Persistence.HumanOversight.DecisionReviewRecord",
      entity =>
      {
        entity.Property<string>("RecommendationId")
          .HasMaxLength(128)
          .HasColumnType("nvarchar(128)");

        entity.Property<string>("AuthorityRole")
          .HasMaxLength(128)
          .HasColumnType("nvarchar(128)");

        entity.Property<string>("AuthoritySubjectId")
          .HasMaxLength(256)
          .HasColumnType("nvarchar(256)");

        entity.Property<string>("CriterionRequestId")
          .HasMaxLength(128)
          .HasColumnType("nvarchar(128)");

        entity.Property<int?>("Disposition")
          .HasColumnType("int");

        entity.Property<string>("EvidenceCaseId")
          .HasMaxLength(128)
          .HasColumnType("nvarchar(128)");

        entity.Property<string>("EvidenceReferenceIdsJson")
          .IsRequired()
          .HasColumnType("nvarchar(max)");

        entity.Property<int>("Status")
          .HasColumnType("int");

        entity.HasKey("RecommendationId");

        entity.ToTable("DecisionReviews");
      });

    modelBuilder.Entity(
      "SmartCities.Infrastructure.Persistence.Citizens.EvidenceReferenceRecord",
      entity =>
      {
        entity.Property<string>("ReportId")
          .HasMaxLength(128)
          .HasColumnType("nvarchar(128)");

        entity.Property<string>("EvidenceId")
          .HasMaxLength(128)
          .HasColumnType("nvarchar(128)");

        entity.Property<int>("Kind")
          .HasColumnType("int");

        entity.Property<DateTimeOffset>("ObservedAtUtc")
          .HasColumnType("datetimeoffset");

        entity.Property<int>("Position")
          .HasColumnType("int");

        entity.Property<string>("SourceReference")
          .IsRequired()
          .HasMaxLength(512)
          .HasColumnType("nvarchar(512)");

        entity.Property<string>("SourceSystem")
          .IsRequired()
          .HasMaxLength(128)
          .HasColumnType("nvarchar(128)");

        entity.HasKey("ReportId", "EvidenceId");

        entity.HasIndex("ReportId", "Position")
          .IsUnique();

        entity.ToTable("CitizenMobilityEvidenceReferences");
      });

    modelBuilder.Entity(
      "SmartCities.Infrastructure.Persistence.Citizens.EvidenceReferenceRecord",
      entity =>
      {
        entity.HasOne(
            "SmartCities.Infrastructure.Persistence.Citizens.CitizenMobilityReportRecord",
            "Report")
          .WithMany("EvidenceReferences")
          .HasForeignKey("ReportId")
          .OnDelete(DeleteBehavior.Cascade)
          .IsRequired();

        entity.Navigation("Report");
      });

    modelBuilder.Entity(
      "SmartCities.Infrastructure.Persistence.Citizens.CitizenMobilityReportRecord",
      entity =>
      {
        entity.Navigation("EvidenceReferences");
      });
  }
}
