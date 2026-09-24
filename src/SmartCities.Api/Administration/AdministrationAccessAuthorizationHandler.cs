using Microsoft.AspNetCore.Authorization;
using SmartCities.Application.Administration;
using SmartCities.Identity;

namespace SmartCities.Api.Administration;

internal sealed class AdministrationAccessAuthorizationHandler
  : AuthorizationHandler<AdministrationAccessRequirement>
{
  private readonly IAdministrationAccessService administration;

  public AdministrationAccessAuthorizationHandler(
    IAdministrationAccessService administration)
  {
    ArgumentNullException.ThrowIfNull(administration);
    this.administration = administration;
  }

  protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    AdministrationAccessRequirement requirement)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(requirement);

    if (context.User.Identity?.IsAuthenticated != true)
    {
      return;
    }

    CanonicalIdentity canonical;

    try
    {
      canonical = context.User.ToCanonicalIdentity();
    }
    catch (InvalidOperationException)
    {
      return;
    }

    var identity = AdministrationIdentity.Create(
      canonical.SubjectId,
      canonical.EmailAddress);

    if (await administration
      .IsAuthorizedAsync(
        identity,
        CancellationToken.None)
      .ConfigureAwait(false))
    {
      context.Succeed(requirement);
    }
  }
}
