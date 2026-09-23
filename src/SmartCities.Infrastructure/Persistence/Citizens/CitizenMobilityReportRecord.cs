namespace SmartCities.Infrastructure.Persistence.Citizens;

internal sealed class CitizenMobilityReportRecord
{
  public string ReportId { get; set; } = string.Empty;

  public string CaseId { get; set; } = string.Empty;

  public string Subject { get; set; } = string.Empty;

  public List<EvidenceReferenceRecord> EvidenceReferences { get; } = [];
}
