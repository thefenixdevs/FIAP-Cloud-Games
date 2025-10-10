using Microsoft.AspNetCore.Authorization;

namespace GameStore.API.Authorization;

public class ConfirmedCommonUserHandler : AuthorizationHandler<ConfirmedCommonUserRequirement>
{
  protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      ConfirmedCommonUserRequirement requirement)
  {
    var profileTypeClaim = context.User.FindFirst("ProfileType");
    var accountStatusClaim = context.User.FindFirst("AccountStatus");

    if (accountStatusClaim?.Value == "Active" &&
        (profileTypeClaim?.Value == "CommonUser" || profileTypeClaim?.Value == "Admin"))
    {
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}
