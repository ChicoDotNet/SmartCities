using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartCities.Infrastructure.Persistence;

#nullable disable

namespace SmartCities.Infrastructure.PostgreSql.Migrations;

/// <summary>
/// Creates the first PostgreSQL persistence shape for idempotent citizen mobility report acceptance.
/// </summary>
[DbContext(typeof(SmartCitiesDbContext))]
[Migration("20260923121000_InitialCitizenMobilityPersistence")]
public sealed class InitialCitizenMobilityPersistence : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
      name: "CitizenMobilityReports",
      columns: table => new
      {
        ReportId = table.Column<string>(
          type: "character varying(128)",
          maxLength: 128,
          nullable: false),
        CaseId = table.Column<string>(
          type: "character varying(128)",
          maxLength: 128,
          nullable: false),
        Subject = table.Column<string>(
          type: "character varying(2048)",
          maxLength: 2048,
          nullable: false),
      },
      constraints: table =>
      {
        table.PrimaryKey(
          "PK_CitizenMobilityReports",
          item => item.ReportId);
      });

    migrationBuilder.CreateTable(
      name: "CitizenMobilityEvidenceReferences",
      columns: table => new
      {
        ReportId = table.Column<string>(
          type: "character varying(128)",
          maxLength: 128,
          nullable: false),
        EvidenceId = table.Column<string>(
          type: "character varying(128)",
          maxLength: 128,
          nullable: false),
        Position = table.Column<int>(
          type: "integer",
          nullable: false),
        Kind = table.Column<int>(
          type: "integer",
          nullable: false),
        SourceSystem = table.Column<string>(
          type: "character varying(128)",
          maxLength: 128,
          nullable: false),
        SourceReference = table.Column<string>(
          type: "character varying(512)",
          maxLength: 512,
          nullable: false),
        ObservedAtUtc = table.Column<DateTimeOffset>(
          type: "timestamp with time zone",
          nullable: false),
      },
      constraints: table =>
      {
        table.PrimaryKey(
          "PK_CitizenMobilityEvidenceReferences",
          item => new
          {
            item.ReportId,
            item.EvidenceId,
          });

        table.ForeignKey(
          name: "FK_CitizenMobilityEvidenceReferences_CitizenMobilityReports_ReportId",
          column: item => item.ReportId,
          principalTable: "CitizenMobilityReports",
          principalColumn: "ReportId",
          onDelete: ReferentialAction.Cascade);
      });

    migrationBuilder.CreateIndex(
      name: "IX_CitizenMobilityEvidenceReferences_ReportId_Position",
      table: "CitizenMobilityEvidenceReferences",
      columns: ["ReportId", "Position"],
      unique: true);

    migrationBuilder.CreateIndex(
      name: "IX_CitizenMobilityReports_CaseId",
      table: "CitizenMobilityReports",
      column: "CaseId",
      unique: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
      name: "CitizenMobilityEvidenceReferences");

    migrationBuilder.DropTable(
      name: "CitizenMobilityReports");
  }
}
