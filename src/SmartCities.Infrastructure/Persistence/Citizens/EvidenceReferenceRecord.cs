using SmartCities.Evidence;

namespace SmartCities.Infrastructure.Persistence.Citizens;

internal sealed class EvidenceReferenceRecord
{
  public string ReportId { get; set; } = string.Empty;

  public string EvidenceId { get; set; } = string.Empty;

  public int Position { get; set; }

  public EvidenceKind Kind { get; set; }

  public string SourceSystem { get; set; } = string.Empty;

  public string SourceReference { get; set; } = string.Empty;

  public DateTimeOffset ObservedAtUtc { get; set; }

  public CitizenMobilityReportRecord Report { get; set; } = null!;
}
