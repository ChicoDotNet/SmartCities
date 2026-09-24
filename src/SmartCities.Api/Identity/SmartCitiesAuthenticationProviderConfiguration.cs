using Microsoft.Extensions.Configuration;

namespace SmartCities.Api.Identity;

internal sealed record LocalAuthenticationProviderConfiguration(
  string Issuer,
  string Audience,
  string SigningKey,
  string CookieName,
  string SubjectClaimType,
  string RoleClaimType,
  string PermissionClaimType,
  int JwtLifetimeMinutes,
  IReadOnlyList<string> DefaultAuthorityRoles,
  IReadOnlyList<string> DefaultPermissions,
  IReadOnlySet<string> AllowedAuthorityRoles,
  IReadOnlySet<string> AllowedPermissions);

internal sealed record OidcAuthenticationProviderConfiguration(
  string ProviderId,
  string DisplayName,
  string Authority,
  string ClientId,
  string ClientSecret,
  string CallbackPath,
  string SubjectClaimType,
  string? TenantClaimType,
  string? RoleClaimType,
  string? PermissionClaimType,
  IReadOnlyList<string> DefaultAuthorityRoles,
  IReadOnlyList<string> DefaultPermissions,
  IReadOnlyDictionary<string, string> RoleMappings,
  IReadOnlyDictionary<string, string> PermissionMappings)
{
  internal string SchemeName =>
    SmartCitiesAuthenticationSchemes.Oidc(
      ProviderId);
}

internal sealed record AuthenticationProviderConfigurationSet(
  LocalAuthenticationProviderConfiguration? Local,
  IReadOnlyList<OidcAuthenticationProviderConfiguration> OpenIdConnect)
{
  internal bool RequiresSession =>
    Local is not null || OpenIdConnect.Count > 0;
}

internal static class SmartCitiesAuthenticationProviderConfiguration
{
  private const string Root =
    "SmartCities:Authentication";

  internal static AuthenticationProviderConfigurationSet Read(
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    var root = configuration.GetSection(Root);
    var local = ReadLocal(
      root.GetSection("Local"));
    var oidc = root
      .GetSection("OpenIdConnect")
      .GetChildren()
      .Select(ReadOidc)
      .Where(
        static item => item is not null)
      .Cast<OidcAuthenticationProviderConfiguration>()
      .ToArray();

    ValidateProviderIdentifiers(oidc);
    ValidateCallbacks(oidc);

    return new AuthenticationProviderConfigurationSet(
      local,
      oidc);
  }

  private static LocalAuthenticationProviderConfiguration? ReadLocal(
    IConfigurationSection section)
  {
    if (!IsEnabled(section))
    {
      return null;
    }

    var issuer = Required(section, "Issuer", "Local");
    var audience = Required(section, "Audience", "Local");
    var signingKey = Required(section, "SigningKey", "Local");

    if (System.Text.Encoding.UTF8.GetByteCount(signingKey) < 32)
    {
      throw new InvalidOperationException(
        "Local authentication SigningKey must contain at least 32 UTF-8 bytes.");
    }

    return new LocalAuthenticationProviderConfiguration(
      issuer,
      audience,
      signingKey,
      section["CookieName"]?.Trim()
        is { Length: > 0 } cookieName
          ? cookieName
          : "smartcities.session",
      Optional(section, "SubjectClaimType", "sub"),
      Optional(section, "RoleClaimType", "role"),
      Optional(section, "PermissionClaimType", "permission"),
      ReadInt(
        section,
        "JwtLifetimeMinutes",
        defaultValue: 60,
        minimum: 1,
        maximum: 1440,
        providerId: "Local"),
      ReadList(section.GetSection("DefaultAuthorityRoles")),
      ReadList(section.GetSection("DefaultPermissions")),
      ReadSet(section.GetSection("AllowedAuthorityRoles")),
      ReadSet(section.GetSection("AllowedPermissions")));
  }

  private static OidcAuthenticationProviderConfiguration? ReadOidc(
    IConfigurationSection section)
  {
    if (!IsEnabled(section))
    {
      return null;
    }

    ValidateProviderId(section.Key);

    var authority = Required(
      section,
      "Authority",
      section.Key);

    if (!Uri.TryCreate(
        authority,
        UriKind.Absolute,
        out var authorityUri)
      || authorityUri.Scheme != Uri.UriSchemeHttps)
    {
      throw new InvalidOperationException(
        $"OIDC provider '{section.Key}' Authority must be an absolute HTTPS URI.");
    }

    var callbackPath = Required(
      section,
      "CallbackPath",
      section.Key);

    if (callbackPath.Length == 0
      || callbackPath[0] != '/'
      || callbackPath.Contains('?'))
    {
      throw new InvalidOperationException(
        $"OIDC provider '{section.Key}' CallbackPath must be an absolute application path without a query string.");
    }

    return new OidcAuthenticationProviderConfiguration(
      section.Key,
      section["DisplayName"]?.Trim()
        is { Length: > 0 } displayName
          ? displayName
          : section.Key,
      authorityUri.AbsoluteUri.TrimEnd('/'),
      Required(section, "ClientId", section.Key),
      Required(section, "ClientSecret", section.Key),
      callbackPath,
      Optional(section, "SubjectClaimType", "sub"),
      OptionalNullable(section, "TenantClaimType"),
      OptionalNullable(section, "RoleClaimType"),
      OptionalNullable(section, "PermissionClaimType"),
      ReadList(section.GetSection("DefaultAuthorityRoles")),
      ReadList(section.GetSection("DefaultPermissions")),
      ReadMap(section.GetSection("RoleMappings")),
      ReadMap(section.GetSection("PermissionMappings")));
  }

  private static bool IsEnabled(
    IConfigurationSection section)
  {
    if (!section.Exists())
    {
      return false;
    }

    var enabled = section["Enabled"];

    if (enabled is null)
    {
      return true;
    }

    if (!bool.TryParse(enabled, out var parsed))
    {
      throw new InvalidOperationException(
        $"Authentication provider section '{section.Path}' has an invalid Enabled value.");
    }

    return parsed;
  }

  private static string Required(
    IConfigurationSection section,
    string key,
    string providerId)
  {
    var value = section[key];

    if (string.IsNullOrWhiteSpace(value))
    {
      throw new InvalidOperationException(
        $"Authentication provider '{providerId}' requires '{key}'.");
    }

    return value.Trim();
  }

  private static string Optional(
    IConfigurationSection section,
    string key,
    string fallback) =>
    section[key]?.Trim()
      is { Length: > 0 } value
        ? value
        : fallback;

  private static int ReadInt(
    IConfigurationSection section,
    string key,
    int defaultValue,
    int minimum,
    int maximum,
    string providerId)
  {
    var configured = section[key];

    if (string.IsNullOrWhiteSpace(configured))
    {
      return defaultValue;
    }

    if (!int.TryParse(
        configured,
        System.Globalization.NumberStyles.Integer,
        System.Globalization.CultureInfo.InvariantCulture,
        out var value)
      || value < minimum
      || value > maximum)
    {
      throw new InvalidOperationException(
        $"Authentication provider '{providerId}' requires '{key}' between {minimum} and {maximum}.");
    }

    return value;
  }

  private static string? OptionalNullable(
    IConfigurationSection section,
    string key) =>
    section[key]?.Trim()
      is { Length: > 0 } value
        ? value
        : null;

  private static string[] ReadList(
    IConfigurationSection section) =>
    section
      .GetChildren()
      .Select(static item => item.Value?.Trim())
      .Where(
        static value =>
          !string.IsNullOrWhiteSpace(value))
      .Cast<string>()
      .Distinct(StringComparer.Ordinal)
      .ToArray();

  private static HashSet<string> ReadSet(
    IConfigurationSection section) =>
    new(
      ReadList(section),
      StringComparer.Ordinal);

  private static Dictionary<string, string> ReadMap(
    IConfigurationSection section)
  {
    var result =
      new Dictionary<string, string>(
        StringComparer.Ordinal);

    foreach (var child in section.GetChildren())
    {
      if (string.IsNullOrWhiteSpace(child.Value))
      {
        throw new InvalidOperationException(
          $"Authentication mapping '{child.Path}' cannot be empty.");
      }

      result.Add(
        child.Key,
        child.Value.Trim());
    }

    return result;
  }

  private static void ValidateProviderIdentifiers(
    IReadOnlyList<OidcAuthenticationProviderConfiguration> providers)
  {
    if (providers.Any(
      static item =>
        string.Equals(
          item.ProviderId,
          "local",
          StringComparison.OrdinalIgnoreCase)))
    {
      throw new InvalidOperationException(
        "OIDC provider identifier 'local' is reserved.");
    }

    if (providers
      .GroupBy(
        static item => item.ProviderId,
        StringComparer.OrdinalIgnoreCase)
      .Any(static group => group.Count() > 1))
    {
      throw new InvalidOperationException(
        "OIDC provider identifiers must be unique ignoring case.");
    }
  }

  private static void ValidateCallbacks(
    IReadOnlyList<OidcAuthenticationProviderConfiguration> providers)
  {
    if (providers
      .GroupBy(
        static item => item.CallbackPath,
        StringComparer.OrdinalIgnoreCase)
      .Any(static group => group.Count() > 1))
    {
      throw new InvalidOperationException(
        "OIDC provider CallbackPath values must be unique.");
    }
  }

  private static void ValidateProviderId(
    string providerId)
  {
    if (providerId.Length is < 1 or > 64
      || providerId.Any(
        static character =>
          !char.IsAsciiLetterOrDigit(character)
          && character is not '-' and not '_' and not '.'))
    {
      throw new InvalidOperationException(
        $"OIDC provider identifier '{providerId}' contains unsupported characters.");
    }
  }
}
