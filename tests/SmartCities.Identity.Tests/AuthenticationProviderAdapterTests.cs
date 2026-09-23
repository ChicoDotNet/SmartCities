using System.Security.Claims;
using SmartCities.Identity;
using Xunit;

namespace SmartCities.Identity.Tests;

public sealed class AuthenticationProviderAdapterTests
{
  [Fact]
  public void Resolver_selects_the_single_adapter_that_accepts_the_provider_context()
  {
    var entra = new RecordingAdapter(
      "entra",
      issuer: "https://login.microsoftonline.com/tenant-a/v2.0");
    var keycloak = new RecordingAdapter(
      "keycloak",
      issuer: "https://identity.city.example/realms/citizens");
    var resolver = new AuthenticationProviderAdapterResolver(
      [entra, keycloak]);
    var context = AuthenticationProviderContext.Create(
      authenticationScheme: "Bearer",
      issuer: "https://identity.city.example/realms/citizens",
      tenantId: "city-a");

    var resolved = resolver.Resolve(context);

    Assert.Same(keycloak, resolved);
  }

  [Fact]
  public void Resolver_rejects_an_unknown_issuer_instead_of_guessing_an_adapter()
  {
    var resolver = new AuthenticationProviderAdapterResolver(
      [
        new RecordingAdapter(
          "entra",
          issuer: "https://login.microsoftonline.com/tenant-a/v2.0"),
      ]);
    var context = AuthenticationProviderContext.Create(
      authenticationScheme: "Bearer",
      issuer: "https://unknown.example",
      tenantId: null);

    Assert.Throws<InvalidOperationException>(
      () => resolver.Resolve(context));
  }

  [Fact]
  public void Resolver_rejects_ambiguous_provider_matches()
  {
    const string issuer =
      "https://identity.city.example/realms/shared";
    var resolver = new AuthenticationProviderAdapterResolver(
      [
        new RecordingAdapter("adapter-a", issuer),
        new RecordingAdapter("adapter-b", issuer),
      ]);
    var context = AuthenticationProviderContext.Create(
      authenticationScheme: "Bearer",
      issuer,
      tenantId: "city-a");

    Assert.Throws<InvalidOperationException>(
      () => resolver.Resolve(context));
  }

  [Fact]
  public async Task Adapter_normalizes_provider_identity_into_canonical_claims_only()
  {
    var adapter = new RecordingAdapter(
      "provider-a",
      issuer: "https://idp-a.example");
    var context = AuthenticationProviderContext.Create(
      authenticationScheme: "Bearer",
      issuer: "https://idp-a.example",
      tenantId: "tenant-17");
    var external = ExternalAuthenticatedIdentity.Create(
      subject: "provider-native-subject",
      claims:
      [
        ExternalIdentityClaim.Create(
          "role",
          "city-reviewer"),
        ExternalIdentityClaim.Create(
          "scope",
          "reviews.finalize"),
      ]);

    var canonical = await adapter.NormalizeAsync(
      context,
      external,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      "provider-a",
      canonical.IdentityProvider);
    Assert.Equal(
      "provider-a:tenant-17:provider-native-subject",
      canonical.SubjectId);
    Assert.Equal(
      ["mobility-reviewer"],
      canonical.AuthorityRoles);
    Assert.Equal(
      [SmartCitiesPermissions.FinalizeDecisionReview],
      canonical.Permissions);

    ClaimsPrincipal principal = canonical.ToClaimsPrincipal(
      authenticationType: "Bearer");

    Assert.True(principal.Identity?.IsAuthenticated);
    Assert.Equal(
      canonical.SubjectId,
      Assert.Single(
        principal.FindAll(
          SmartCitiesClaimTypes.Subject)).Value);
    Assert.Equal(
      "provider-a",
      Assert.Single(
        principal.FindAll(
          SmartCitiesClaimTypes.IdentityProvider)).Value);
    Assert.Empty(
      principal.FindAll("role"));
    Assert.Empty(
      principal.FindAll("scope"));
  }

  [Fact]
  public void Canonical_identity_rejects_duplicate_or_blank_authorization_values()
  {
    Assert.Throws<ArgumentException>(
      () => CanonicalIdentity.Create(
        identityProvider: "provider-a",
        subjectId: "subject-1",
        authorityRoles:
        [
          "mobility-reviewer",
          "mobility-reviewer",
        ],
        permissions:
        [
          SmartCitiesPermissions.FinalizeDecisionReview,
        ]));

    Assert.Throws<ArgumentException>(
      () => CanonicalIdentity.Create(
        identityProvider: "provider-a",
        subjectId: "subject-1",
        authorityRoles:
        [
          " ",
        ],
        permissions:
        [
          SmartCitiesPermissions.FinalizeDecisionReview,
        ]));
  }

  private sealed class RecordingAdapter : IAuthenticationProviderAdapter
  {
    private readonly string issuer;

    public RecordingAdapter(
      string providerId,
      string issuer)
    {
      ProviderId = providerId;
      this.issuer = issuer;
    }

    public string ProviderId { get; }

    public bool CanHandle(
      AuthenticationProviderContext context) =>
      string.Equals(
        issuer,
        context.Issuer,
        StringComparison.Ordinal);

    public Task<CanonicalIdentity> NormalizeAsync(
      AuthenticationProviderContext context,
      ExternalAuthenticatedIdentity identity,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var tenant = context.TenantId ?? "default";

      return Task.FromResult(
        CanonicalIdentity.Create(
          ProviderId,
          $"{ProviderId}:{tenant}:{identity.Subject}",
          ["mobility-reviewer"],
          [SmartCitiesPermissions.FinalizeDecisionReview]));
    }
  }
}
