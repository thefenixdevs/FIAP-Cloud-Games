using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Infrastructure.Services.Authentication;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameStore.Tests.Application.Services;

public class JwtServiceTests
{
  private readonly Mock<ILogger<JwtService>> _loggerMock;
  private readonly IConfiguration _configuration;

  private User _commonUserActive;
  private User _adminUserActive;
  public JwtServiceTests()
  {
    _loggerMock = new Mock<ILogger<JwtService>>();

    var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "ThisIsAVerySecretKeyForTestingPurposesOnly123456"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpirationInMinutes", "60"}
        };

    _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(inMemorySettings!)
        .Build();

    var password = "Teste123!";
    var testHasher = new TestPasswordHasher();

    var commonUser = User.Register("Test User", "test@example.com", "testuser", password, testHasher);
    commonUser.ConfirmAccount();
    _commonUserActive = commonUser;

    var adminUser = User.Register("Admin User", "admin@example.com", "adminuser", password, testHasher, ProfileType.Admin);
    adminUser.ConfirmAccount();
    _adminUserActive = adminUser;
  }

  [Fact]
  public void GenerateToken_ShouldReturnValidToken_WhenUserIsValid()
  {
    var jwtService = new JwtService(_configuration, _loggerMock.Object);
    var token = jwtService.GenerateToken(_commonUserActive);

    Assert.NotNull(token);
    Assert.NotEmpty(token);
  }

  [Fact]
  public void GenerateToken_ShouldIncludeCorrectClaims_WhenTokenGenerated()
  {
    var jwtService = new JwtService(_configuration, _loggerMock.Object);

    var token = jwtService.GenerateToken(_adminUserActive);
    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(token);

    Assert.Equal(_adminUserActive.Id.ToString(), jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
    Assert.Equal("admin@example.com", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    Assert.Equal("adminuser", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);
    Assert.Equal("Admin", jwtToken.Claims.First(c => c.Type == "ProfileType").Value);
    Assert.Equal("Active", jwtToken.Claims.First(c => c.Type == "AccountStatus").Value);
  }

  [Fact]
  public void GenerateToken_ShouldSetCorrectIssuerAndAudience()
  {
    var jwtService = new JwtService(_configuration, _loggerMock.Object);

    var token = jwtService.GenerateToken(_commonUserActive);
    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(token);

    Assert.Equal("TestIssuer", jwtToken.Issuer);
    Assert.Contains("TestAudience", jwtToken.Audiences);
  }

  [Fact]
  public void GenerateToken_ShouldSetExpirationTime()
  {
    var jwtService = new JwtService(_configuration, _loggerMock.Object);

    var beforeGeneration = DateTime.UtcNow;
    var token = jwtService.GenerateToken(_commonUserActive);
    var afterGeneration = DateTime.UtcNow;

    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(token);

    Assert.True(jwtToken.ValidTo > beforeGeneration.AddMinutes(59));
    Assert.True(jwtToken.ValidTo <= afterGeneration.AddMinutes(61));
  }

  [Fact]
  public void GenerateToken_ShouldThrowException_WhenSecretKeyIsMissing()
  {
    var invalidConfig = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Jwt:ExpirationInMinutes", "60"}
        }!)
        .Build();

    var jwtService = new JwtService(invalidConfig, _loggerMock.Object);

    Assert.Throws<InvalidOperationException>(() => jwtService.GenerateToken(_commonUserActive));
  }

  [Fact]
  public void GenerateToken_ShouldLogInformation_WhenTokenGeneratedSuccessfully()
  {
    var jwtService = new JwtService(_configuration, _loggerMock.Object);

    jwtService.GenerateToken(_commonUserActive);

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("JWT token generated successfully")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }
}
