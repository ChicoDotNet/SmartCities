using Microsoft.EntityFrameworkCore;
using SmartCities.Application.Citizens;
using SmartCities.Evidence;

namespace SmartCities.Infrastructure.Persistence.Citizens;

/// <summary>
/// Persists citizen mobility report acceptance through EF Core.
/// </summary>
/// <remarks>
/// The report identifier is the idempotency key. The database primary-key constraint is the final arbitration
/// point for competing inserts; when another transaction wins, this repository reloads and returns that
/// authoritative case.
/// </remarks>
public sealed class EfCitizenMobilityReportRepository
  : ICitizenMobilityReportRepository
{
  private readonly SmartCitiesDbContext dbContext;

  /// <summary>Initializes the repository with the current EF Core Unit of Work.</summary>
  /// <param name="dbContext">Scoped persistence session shared by the application operation.</param>
  public EfCitizenMobilityReportRepository(
    SmartCitiesDbContext dbContext)
  {
    ArgumentNullException.ThrowIfNull(dbContext);
    this.dbContext = dbContext;
  }

  /// <inheritdoc />
  public async Task<CitizenMobilityReportAcceptance> GetOrCreateAsync(
    string reportId,
    EvidenceCase candidateCase,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reportId);
    ArgumentNullException.ThrowIfNull(candidateCase);

    var existing = await FindAsync(reportId, cancellationToken)
      .ConfigureAwait(false);

    if (existing is not null)
    {
      return CitizenMobilityReportAcceptance.Existing(
        reportId,
        ToEvidenceCase(existing));
    }

    var record = ToRecord(reportId, candidateCase);
    dbContext.CitizenMobilityReports.Add(record);

    try
    {
      await dbContext.SaveChangesAsync(cancellationToken)
        .ConfigureAwait(false);

      return CitizenMobilityReportAcceptance.Created(
        reportId,
        candidateCase);
    }
    catch (DbUpdateException)
    {
      dbContext.ChangeTracker.Clear();

      existing = await FindAsync(reportId, cancellationToken)
        .ConfigureAwait(false);

      if (existing is null)
      {
        throw;
      }

      return CitizenMobilityReportAcceptance.Existing(
        reportId,
        ToEvidenceCase(existing));
    }
  }

  private Task<CitizenMobilityReportRecord?> FindAsync(
    string reportId,
    CancellationToken cancellationToken) =>
    dbContext.CitizenMobilityReports
      .AsNoTracking()
      .Include(static entity => entity.EvidenceReferences)
      .SingleOrDefaultAsync(
        entity => entity.ReportId == reportId,
        cancellationToken);

  private static CitizenMobilityReportRecord ToRecord(
    string reportId,
    EvidenceCase evidenceCase)
  {
    var record = new CitizenMobilityReportRecord
    {
      ReportId = reportId,
      CaseId = evidenceCase.CaseId,
      Subject = evidenceCase.Subject,
    };

    for (var index = 0; index < evidenceCase.EvidenceReferences.Count; index++)
    {
      var evidence = evidenceCase.EvidenceReferences[index];

      record.EvidenceReferences.Add(
        new EvidenceReferenceRecord
        {
          ReportId = reportId,
          EvidenceId = evidence.EvidenceId,
          Position = index,
          Kind = evidence.Kind,
          SourceSystem = evidence.Provenance.SourceSystem,
          SourceReference = evidence.Provenance.SourceReference,
          ObservedAtUtc = evidence.Provenance.ObservedAtUtc,
        });
    }

    return record;
  }

  private static EvidenceCase ToEvidenceCase(
    CitizenMobilityReportRecord record)
  {
    var evidence = record.EvidenceReferences
      .OrderBy(static item => item.Position)
      .Select(static item =>
        EvidenceReference.Create(
          item.EvidenceId,
          item.Kind,
          EvidenceProvenance.Create(
            item.SourceSystem,
            item.SourceReference,
            item.ObservedAtUtc)))
      .ToArray();

    return EvidenceCase.Create(
      record.CaseId,
      record.Subject,
      evidence);
  }
}
