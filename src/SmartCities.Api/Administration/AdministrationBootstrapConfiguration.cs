using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartCities.Api.Administration;

/// <summary>
/// Holds validated deployment bootstrap settings without exposing the configured secret through public APIs.
/// </summary>
public sealed class AdministrationBootstrapConfiguration
{
  internal AdministrationBootstrapConfiguration(
    string userName,
    string? password)
  {
    UserName = userName;
    Password = password;
  }

  internal string UserName { get; }

  internal string? Password { get; }

  internal bool IsEnabled =>
    Password is not null;

  internal static AdministrationBootstrapConfiguration Read(
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    var section = configuration.GetSection(
      "SmartCities:Administration:Bootstrap");
    var userName = section["UserName"]?.Trim()
      is { Length: > 0 } configuredUserName
        ? configuredUserName
        : AdministrationAccessService.BootstrapEmailAddress;
    var password = section["Password"];

    if (password is not null)
    {
      password = password.Trim();

      if (password.Length < 16)
      {
        throw new InvalidOperationException(
          "Administration bootstrap Password must contain at least 16 characters when configured.");
      }
    }

    return new AdministrationBootstrapConfiguration(
      userName,
      string.IsNullOrWhiteSpace(password)
        ? null
        : password);
  }
}

/// <summary>
/// Registers the optional empty-whitelist Administration bootstrap credential.
/// </summary>
public static class AdministrationBootstrapExtensions
{
  /// <summary>Registers validated Administration bootstrap configuration.</summary>
  public static IServiceCollection AddSmartCitiesAdministrationBootstrap(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddSingleton(
      AdministrationBootstrapConfiguration.Read(
        configuration));

    return services;
  }
}
