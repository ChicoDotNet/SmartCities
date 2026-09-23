using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartCities.Infrastructure.Persistence;

#nullable disable

namespace SmartCities.Infrastructure.PostgreSql.Migrations;

/// <summary>
/// PostgreSQL model snapshot for the provider-specific migration chain.
/// </summary>
[DbContext(typeof(SmartCitiesDbContext))]
public sealed class SmartCitiesDbContextModelSnapshot : ModelSnapshot
{
  /// <inheritdoc />
  protected override void BuildModel(ModelBuilder modelBuilder)
  {
    modelBuilder
      .HasAnnotation("ProductVersion", "10.0.12")
      .HasAnnotation("Relational:MaxIdentifierLength", 63);

    modelBuilder.Entity(
      "SmartCities.Infrastructure.Persistence.Citizens.CitizenMobilityReportRecord",
      entity =>
      {
        entity.Property<string>("ReportId")
          .HasMaxLength(128)
          .HasColumnType("character varying(128)");

        entity.Property<string>("CaseId")
          .IsRequired()
          .HasMaxLength(128)
          .HasColumnType("character varying(128)");

        entity.Property<string>("Subject")
          .IsRequired()
          .HasMaxLength(2048)
          .HasColumnType("character varying(2048)");

        entity.HasKey("ReportId");

        entity.HasIndex("CaseId")
          .IsUnique();

        entity.ToTable("CitizenMobilityReports");
      });

    modelBuilder.Entity(
      "SmartCities.Infrastructure.Persistence.Citizens.EvidenceReferenceRecord",
      entity =>
      {
        entity.Property<string>("ReportId")
          .HasMaxLength(128)
          .HasColumnType("character varying(128)");

        entity.Property<string>("EvidenceId")
          .HasMaxLength(128)
          .HasColumnType("character varying(128)");

        entity.Property<int>("Kind")
          .HasColumnType("integer");

        entity.Property<DateTimeOffset>("ObservedAtUtc")
          .HasColumnType("timestamp with time zone");

        entity.Property<int>("Position")
          .HasColumnType("integer");

        entity.Property<string>("SourceReference")
          .IsRequired()
          .HasMaxLength(512)
          .HasColumnType("character varying(512)");

        entity.Property<string>("SourceSystem")
          .IsRequired()
          .HasMaxLength(128)
          .HasColumnType("character varying(128)");

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
