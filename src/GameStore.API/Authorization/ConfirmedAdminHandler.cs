using Microsoft.AspNetCore.Authorization;

namespace GameStore.API.Authorization;

public class ConfirmedAdminHandler : AuthorizationHandler<ConfirmedAdminRequirement>
{
  protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      ConfirmedAdminRequirement requirement)
  {
    var profileTypeClaim = context.User.FindFirst("ProfileType");
    var accountStatusClaim = context.User.FindFirst("AccountStatus");

    if (profileTypeClaim?.Value == "Admin" && accountStatusClaim?.Value == "Active")
    {
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}
