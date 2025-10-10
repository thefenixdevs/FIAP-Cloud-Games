using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using GameStore.API.Authorization;
using Xunit;

namespace GameStore.Tests.API.Authorization;

public class AuthorizationHandlerTests
{
  [Fact]
  public async Task ConfirmedAdminHandler_ShouldSucceed_WhenUserIsActiveAdmin()
  {
    var handler = new ConfirmedAdminHandler();
    var requirement = new ConfirmedAdminRequirement();

    var claims = new[]
    {
            new Claim("ProfileType", "Admin"),
            new Claim("AccountStatus", "Active")
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var context = new AuthorizationHandlerContext(
        new[] { requirement },
        claimsPrincipal,
        null);

    await handler.HandleAsync(context);

    Assert.True(context.HasSucceeded);
  }

  [Fact]
  public async Task ConfirmedAdminHandler_ShouldFail_WhenUserIsNotConfirmed()
  {
    var handler = new ConfirmedAdminHandler();
    var requirement = new ConfirmedAdminRequirement();

    var claims = new[]
    {
            new Claim("ProfileType", "Admin"),
            new Claim("AccountStatus", "Pending")
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var context = new AuthorizationHandlerContext(
        new[] { requirement },
        claimsPrincipal,
        null);

    await handler.HandleAsync(context);

    Assert.False(context.HasSucceeded);
  }

  [Fact]
  public async Task ConfirmedAdminHandler_ShouldFail_WhenUserIsNotAdmin()
  {
    var handler = new ConfirmedAdminHandler();
    var requirement = new ConfirmedAdminRequirement();

    var claims = new[]
    {
            new Claim("ProfileType", "CommonUser"),
            new Claim("AccountStatus", "Active")
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var context = new AuthorizationHandlerContext(
        new[] { requirement },
        claimsPrincipal,
        null);

    await handler.HandleAsync(context);

    Assert.False(context.HasSucceeded);
  }

  [Fact]
  public async Task ConfirmedAdminHandler_ShouldFail_WhenUserIsBanned()
  {
    var handler = new ConfirmedAdminHandler();
    var requirement = new ConfirmedAdminRequirement();

    var claims = new[]
    {
            new Claim("ProfileType", "Admin"),
            new Claim("AccountStatus", "Banned")
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var context = new AuthorizationHandlerContext(
        new[] { requirement },
        claimsPrincipal,
        null);

    await handler.HandleAsync(context);

    Assert.False(context.HasSucceeded);
  }

  [Fact]
  public async Task ConfirmedCommonUserHandler_ShouldSucceed_WhenUserIsActiveCommonUser()
  {
    var handler = new ConfirmedCommonUserHandler();
    var requirement = new ConfirmedCommonUserRequirement();

    var claims = new[]
    {
            new Claim("ProfileType", "CommonUser"),
            new Claim("AccountStatus", "Active")
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var context = new AuthorizationHandlerContext(
        new[] { requirement },
        claimsPrincipal,
        null);

    await handler.HandleAsync(context);

    Assert.True(context.HasSucceeded);
  }

  [Fact]
  public async Task ConfirmedCommonUserHandler_ShouldSucceed_WhenUserIsActiveAdmin()
  {
    var handler = new ConfirmedCommonUserHandler();
    var requirement = new ConfirmedCommonUserRequirement();

    var claims = new[]
    {
            new Claim("ProfileType", "Admin"),
            new Claim("AccountStatus", "Active")
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var context = new AuthorizationHandlerContext(
        new[] { requirement },
        claimsPrincipal,
        null);

    await handler.HandleAsync(context);

    Assert.True(context.HasSucceeded);
  }

  [Fact]
  public async Task ConfirmedCommonUserHandler_ShouldFail_WhenUserIsNotConfirmed()
  {
    var handler = new ConfirmedCommonUserHandler();
    var requirement = new ConfirmedCommonUserRequirement();

    var claims = new[]
    {
            new Claim("ProfileType", "CommonUser"),
            new Claim("AccountStatus", "Pending")
        };
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var context = new AuthorizationHandlerContext(
        new[] { requirement },
        claimsPrincipal,
        null);

    await handler.HandleAsync(context);

    Assert.False(context.HasSucceeded);
  }

  [Fact]
  public async Task ConfirmedCommonUserHandler_ShouldFail_WhenClaimsAreMissing()
  {
    var handler = new ConfirmedCommonUserHandler();
    var requirement = new ConfirmedCommonUserRequirement();

    var claims = Array.Empty<Claim>();
    var identity = new ClaimsIdentity(claims, "TestAuth");
    var claimsPrincipal = new ClaimsPrincipal(identity);

    var context = new AuthorizationHandlerContext(
        new[] { requirement },
        claimsPrincipal,
        null);

    await handler.HandleAsync(context);

    Assert.False(context.HasSucceeded);
  }
}
