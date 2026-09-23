using SmartCities.Evidence;

namespace SmartCities.Application.Citizens;

/// <summary>
/// Defines the persistence boundary for idempotent citizen mobility report acceptance.
/// </summary>
/// <remarks>
/// Implementations must treat <paramref name="reportId"/> as the idempotency key and atomically return the
/// existing authoritative case when that report has already been accepted. Concrete concurrency and transaction
/// guarantees belong to the infrastructure implementation.
/// </remarks>
public interface ICitizenMobilityReportRepository
{
  /// <summary>
  /// Gets the authoritative case for a report, creating it from <paramref name="candidateCase"/> only when the
  /// report has not previously been accepted.
  /// </summary>
  /// <param name="reportId">Stable report identifier used as the idempotency key.</param>
  /// <param name="candidateCase">Candidate Evidence Case for first acceptance.</param>
  /// <param name="cancellationToken">Token used to cancel the operation.</param>
  /// <returns>
  /// The authoritative acceptance result. Replays for the same report identifier return the original case with
  /// <see cref="CitizenMobilityReportAcceptance.WasCreated"/> set to <see langword="false"/>.
  /// </returns>
  Task<CitizenMobilityReportAcceptance> GetOrCreateAsync(
    string reportId,
    EvidenceCase candidateCase,
    CancellationToken cancellationToken = default);
}
