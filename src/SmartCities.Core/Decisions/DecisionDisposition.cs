namespace SmartCities.Decisions;

/// <summary>Represents the final disposition recorded by an accountable human authority.</summary>
public enum DecisionDisposition
{
  /// <summary>The recommendation was accepted as the final disposition.</summary>
  Accepted = 0,

  /// <summary>The recommendation was modified by the accountable human authority.</summary>
  Modified = 1,

  /// <summary>The recommendation was rejected by the accountable human authority.</summary>
  Rejected = 2,
}
