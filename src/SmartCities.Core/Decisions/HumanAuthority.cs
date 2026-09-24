namespace SmartCities.Decisions;

/// <summary>
/// Identifies the accountable human authority that can finalize a civic recommendation.
/// </summary>
public sealed record HumanAuthority
{
  private HumanAuthority(string subjectId, string role)
  {
    SubjectId = subjectId;
    Role = role;
  }

  /// <summary>Gets the stable identifier of the human reviewer or accountable subject.</summary>
  public string SubjectId { get; }

  /// <summary>Gets the role or authority context under which the subject acts.</summary>
  public string Role { get; }

  /// <summary>Creates a validated human-authority identity.</summary>
  /// <param name="subjectId">Stable non-empty identifier for the accountable human.</param>
  /// <param name="role">Non-empty role or authority description.</param>
  /// <returns>A validated immutable authority value.</returns>
  /// <exception cref="ArgumentException">Thrown when <paramref name="subjectId"/> or <paramref name="role"/> is empty or whitespace.</exception>
  public static HumanAuthority Create(string subjectId, string role)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(subjectId);
    ArgumentException.ThrowIfNullOrWhiteSpace(role);

    return new HumanAuthority(subjectId, role);
  }
}
