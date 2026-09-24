namespace SmartCities.Identity;

/// <summary>
/// Represents a fail-closed trust-boundary rejection during authentication canonicalization.
/// </summary>
public sealed class AuthenticationCanonicalizationException
  : InvalidOperationException
{
  /// <summary>Initializes a canonicalization failure with a public-safe operational message.</summary>
  /// <param name="message">Failure description intended for server-side diagnostics.</param>
  public AuthenticationCanonicalizationException(
    string message)
    : base(message)
  {
  }

  /// <summary>Initializes a canonicalization failure with its internal cause.</summary>
  /// <param name="message">Failure description intended for server-side diagnostics.</param>
  /// <param name="innerException">Underlying provider-resolution failure.</param>
  public AuthenticationCanonicalizationException(
    string message,
    Exception innerException)
    : base(message, innerException)
  {
  }
}
