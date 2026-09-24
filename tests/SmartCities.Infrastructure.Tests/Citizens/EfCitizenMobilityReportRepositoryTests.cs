using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartCities.Application.Citizens;
using SmartCities.Citizens;
using SmartCities.Evidence;
using SmartCities.Infrastructure.Persistence;
using SmartCities.Infrastructure.Persistence.Citizens;
using Xunit;

namespace SmartCities.Infrastructure.Tests.Citizens;

public sealed class EfCitizenMobilityReportRepositoryTests
{
  [Fact]
  public async Task Repository_persists_the_first_acceptance_and_replays_the_original_case()
  {
    await using var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync(TestContext.Current.CancellationToken);

    var options = new DbContextOptionsBuilder<SmartCitiesDbContext>()
      .UseSqlite(connection)
      .Options;

    await using (var setup = new SmartCitiesDbContext(options))
    {
      await setup.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    var report = CreateReport("report-001");

    await using (var firstContext = new SmartCitiesDbContext(options))
    {
      var repository =
        new EfCitizenMobilityReportRepository(firstContext);

      var first = await repository.GetOrCreateAsync(
        report.ReportId,
        report.CreateEvidenceCase("case-original"),
        TestContext.Current.CancellationToken);

      Assert.True(first.WasCreated);
      Assert.Equal("case-original", first.EvidenceCase.CaseId);
    }

    await using (var replayContext = new SmartCitiesDbContext(options))
    {
      var repository =
        new EfCitizenMobilityReportRepository(replayContext);

      var replay = await repository.GetOrCreateAsync(
        report.ReportId,
        report.CreateEvidenceCase("case-ignored"),
        TestContext.Current.CancellationToken);

      Assert.False(replay.WasCreated);
      Assert.Equal("case-original", replay.EvidenceCase.CaseId);
    }
  }

  [Fact]
  public async Task Repository_recovers_a_persisted_case_through_a_fresh_context()
  {
    await using var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync(TestContext.Current.CancellationToken);

    var options = new DbContextOptionsBuilder<SmartCitiesDbContext>()
      .UseSqlite(connection)
      .Options;

    await using (var setup = new SmartCitiesDbContext(options))
    {
      await setup.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    var report = CreateReport("report-recover");

    await using (var firstContext = new SmartCitiesDbContext(options))
    {
      var repository = new EfCitizenMobilityReportRepository(firstContext);

      await repository.GetOrCreateAsync(
        report.ReportId,
        report.CreateEvidenceCase("case-persisted"),
        TestContext.Current.CancellationToken);
    }

    await using (var freshContext = new SmartCitiesDbContext(options))
    {
      var repository = new EfCitizenMobilityReportRepository(freshContext);

      var recovered = await repository.GetAsync(
        report.ReportId,
        TestContext.Current.CancellationToken);

      Assert.NotNull(recovered);
      Assert.Equal("report-recover", recovered.ReportId);
      Assert.Equal("case-persisted", recovered.EvidenceCase.CaseId);
      Assert.Equal(report.Description, recovered.EvidenceCase.Subject);
    }
  }

  [Fact]
  public async Task Repository_round_trip_preserves_order_kind_and_provenance()
  {
    await using var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync(TestContext.Current.CancellationToken);

    var options = new DbContextOptionsBuilder<SmartCitiesDbContext>()
      .UseSqlite(connection)
      .Options;

    await using (var setup = new SmartCitiesDbContext(options))
    {
      await setup.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    var report = CitizenMobilityReport.Create(
      "report-002",
      "pedestrian-safety",
      "Intersection AGS-001",
      "Unsafe pedestrian crossing.",
      [
        CreateEvidence(
          "evidence-photo",
          EvidenceKind.Photograph,
          "photo:001",
          new DateTimeOffset(2026, 9, 23, 18, 10, 0, TimeSpan.Zero)),
        CreateEvidence(
          "evidence-statement",
          EvidenceKind.CitizenStatement,
          "statement:001",
          new DateTimeOffset(2026, 9, 23, 18, 11, 0, TimeSpan.Zero)),
      ]);

    await using (var firstContext = new SmartCitiesDbContext(options))
    {
      var repository =
        new EfCitizenMobilityReportRepository(firstContext);

      await repository.GetOrCreateAsync(
        report.ReportId,
        report.CreateEvidenceCase("case-002"),
        TestContext.Current.CancellationToken);
    }

    await using (var replayContext = new SmartCitiesDbContext(options))
    {
      var repository = new EfCitizenMobilityReportRepository(replayContext);
      var replay = await repository.GetOrCreateAsync(
        report.ReportId,
        report.CreateEvidenceCase("case-ignored"),
        TestContext.Current.CancellationToken);

      Assert.False(replay.WasCreated);
      Assert.Equal(
        ["evidence-photo", "evidence-statement"],
        replay.EvidenceCase.EvidenceReferenceIds);

      var photo = replay.EvidenceCase.EvidenceReferences[0];
      Assert.Equal(EvidenceKind.Photograph, photo.Kind);
      Assert.Equal("citizen-portal", photo.Provenance.SourceSystem);
      Assert.Equal("photo:001", photo.Provenance.SourceReference);
      Assert.Equal(
        new DateTimeOffset(2026, 9, 23, 18, 10, 0, TimeSpan.Zero),
        photo.Provenance.ObservedAtUtc);
    }
  }

  private static CitizenMobilityReport CreateReport(string reportId) =>
    CitizenMobilityReport.Create(
      reportId,
      "pedestrian-safety",
      "Intersection AGS-001",
      "Unsafe pedestrian crossing.",
      [
        CreateEvidence(
          "evidence-001",
          EvidenceKind.CitizenStatement,
          $"submission:{reportId}",
          new DateTimeOffset(2026, 9, 23, 18, 0, 0, TimeSpan.Zero)),
      ]);

  private static EvidenceReference CreateEvidence(
    string evidenceId,
    EvidenceKind kind,
    string sourceReference,
    DateTimeOffset observedAtUtc) =>
    EvidenceReference.Create(
      evidenceId,
      kind,
      EvidenceProvenance.Create(
        "citizen-portal",
        sourceReference,
        observedAtUtc));
}
