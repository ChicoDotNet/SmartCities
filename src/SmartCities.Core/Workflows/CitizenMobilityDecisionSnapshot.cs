using SmartCities.Citizens;
using SmartCities.Criterion;
using SmartCities.Decisions;
using SmartCities.Evidence;

namespace SmartCities.Workflows;

/// <summary>
/// Captures one in-memory mobility decision workflow snapshot from citizen report through pending human review.
/// </summary>
/// <remarks>
/// The snapshot is not a persistence aggregate or transaction boundary. It preserves the public traceability
/// chain so later application services can persist the individual domain artifacts through repositories and a Unit of Work.
/// </remarks>
public sealed record CitizenMobilityDecisionSnapshot
{
  internal CitizenMobilityDecisionSnapshot(
    CitizenMobilityReport report,
    EvidenceCase evidenceCase,
    CriterionDecisionRequest criterionRequest,
    CriterionDecisionTrace criterionTrace,
    DecisionReview humanReview)
  {
    Report = report;
    EvidenceCase = evidenceCase;
    CriterionRequest = criterionRequest;
    CriterionTrace = criterionTrace;
    HumanReview = humanReview;
  }

  /// <summary>Gets the originating validated citizen mobility report.</summary>
  public CitizenMobilityReport Report { get; }

  /// <summary>Gets the Evidence Case derived from the report.</summary>
  public EvidenceCase EvidenceCase { get; }

  /// <summary>Gets the bounded Criterion request derived from the Evidence Case.</summary>
  public CriterionDecisionRequest CriterionRequest { get; }

  /// <summary>Gets the advisory Criterion trace returned by the configured provider.</summary>
  public CriterionDecisionTrace CriterionTrace { get; }

  /// <summary>Gets the pending human-review snapshot linked to the Criterion recommendation.</summary>
  public DecisionReview HumanReview { get; }
}
