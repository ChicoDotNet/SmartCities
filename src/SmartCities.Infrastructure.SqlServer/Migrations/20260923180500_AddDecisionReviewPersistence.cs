using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartCities.Infrastructure.Persistence;

#nullable disable

namespace SmartCities.Infrastructure.SqlServer.Migrations;

/// <summary>
/// Adds authoritative human decision-review persistence.
/// </summary>
[DbContext(typeof(SmartCitiesDbContext))]
[Migration("20260923180500_AddDecisionReviewPersistence")]
public sealed class AddDecisionReviewPersistence : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
      name: "DecisionReviews",
      columns: table => new
      {
        RecommendationId = table.Column<string>(
          type: "nvarchar(128)",
          maxLength: 128,
          nullable: false),
        CriterionRequestId = table.Column<string>(
          type: "nvarchar(128)",
          maxLength: 128,
          nullable: true),
        EvidenceCaseId = table.Column<string>(
          type: "nvarchar(128)",
          maxLength: 128,
          nullable: true),
        EvidenceReferenceIdsJson = table.Column<string>(
          type: "nvarchar(max)",
          nullable: false),
        Status = table.Column<int>(
          type: "int",
          nullable: false),
        AuthoritySubjectId = table.Column<string>(
          type: "nvarchar(256)",
          maxLength: 256,
          nullable: true),
        AuthorityRole = table.Column<string>(
          type: "nvarchar(128)",
          maxLength: 128,
          nullable: true),
        Disposition = table.Column<int>(
          type: "int",
          nullable: true),
      },
      constraints: table =>
      {
        table.PrimaryKey(
          "PK_DecisionReviews",
          item => item.RecommendationId);
      });
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
      name: "DecisionReviews");
  }
}
