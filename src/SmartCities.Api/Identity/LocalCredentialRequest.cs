using System.ComponentModel.DataAnnotations;

namespace SmartCities.Api.Identity;

/// <summary>
/// Carries local credentials to the configured deployment-owned authenticator.
/// </summary>
/// <param name="UserName">Local login identifier.</param>
/// <param name="Password">Secret presented for this authentication attempt.</param>
public sealed record LocalCredentialRequest(
  [param: Required] string UserName,
  [param: Required] string Password);
