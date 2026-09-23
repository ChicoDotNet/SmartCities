using SmartCities.Identity;
using Xunit;

namespace SmartCities.Identity.Tests;

public sealed class AuthenticationCanonicalizerTests
{
  [Fact]
  public async Task Canonicalizer_resolves_the_adapter_and_returns_its_canonical_identity()
  {
    var adapter = new StubAdapter(
      providerId: "provider-a",
      acceptedIssuer: "https://idp-a.example",
      canonicalProviderId: "provider-a");
    var canonicalizer = new AuthenticationCanonicalizer(
      new AuthenticationProviderAdapterResolver(
        [adapter]));
    var context = AuthenticationProviderContext.Create(
      "Bearer",
      "https://idp-a.example",
      "tenant-a");
    var external = ExternalAuthenticatedIdentity.Create(
      "external-subject",
      []);

    var canonical = await canonicalizer.CanonicalizeAsync(
      context,
      external,
      TestContext.Current.CancellationToken);

    Assert.Equal(
      "provider-a",
      canonical.IdentityProvider);
    Assert.Equal(
      "provider-a:tenant-a:external-subject",
      canonical.SubjectId);
  }

  [Fact]
  public async Task Canonicalizer_rejects_an_adapter_that_changes_its_provider_identity()
  {
    var adapter = new StubAdapter(
      providerId: "provider-a",
      acceptedIssuer: "https://idp-a.example",
      canonicalProviderId: "provider-b");
    var canonicalizer = new AuthenticationCanonicalizer(
      new AuthenticationProviderAdapterResolver(
        [adapter]));
    var context = AuthenticationProviderContext.Create(
      "Bearer",
      "https://idp-a.example",
      "tenant-a");
    var external = ExternalAuthenticatedIdentity.Create(
      "external-subject",
      []);

    await Assert.ThrowsAsync<AuthenticationCanonicalizationException>(
      () => canonicalizer.CanonicalizeAsync(
        context,
        external,
        TestContext.Current.CancellationToken));
  }

  private sealed class StubAdapter : IAuthenticationProviderAdapter
  {
    private readonly string acceptedIssuer;
    private readonly string canonicalProviderId;

    public StubAdapter(
      string providerId,
      string acceptedIssuer,
      string canonicalProviderId)
    {
      ProviderId = providerId;
      this.acceptedIssuer = acceptedIssuer;
      this.canonicalProviderId = canonicalProviderId;
    }

    public string ProviderId { get; }

    public bool CanHandle(
      AuthenticationProviderContext context) =>
      string.Equals(
        acceptedIssuer,
        context.Issuer,
        StringComparison.Ordinal);

    public Task<CanonicalIdentity> NormalizeAsync(
      AuthenticationProviderContext context,
      ExternalAuthenticatedIdentity identity,
      CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      return Task.FromResult(
        CanonicalIdentity.Create(
          canonicalProviderId,
          $"{ProviderId}:{context.TenantId}:{identity.Subject}",
          ["mobility-reviewer"],
          [SmartCitiesPermissions.FinalizeDecisionReview]));
    }
  }
}
