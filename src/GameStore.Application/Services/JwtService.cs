using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameStore.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GameStore.Application.Services;

public class JwtService : IJwtService
{
  private readonly IConfiguration _configuration;
  private readonly ILogger<JwtService> _logger;

  public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
  {
    _configuration = configuration;
    _logger = logger;
  }

  public string GenerateToken(User user)
  {
    try
    {
      var secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
      var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
      var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
      var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "60");

      var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      var claims = new[]
      {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username.Value),
                new Claim("ProfileType", user.ProfileType.ToString()),
                new Claim("AccountStatus", user.AccountStatus.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

      var token = new JwtSecurityToken(
          issuer: issuer,
          audience: audience,
          claims: claims,
          expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
          signingCredentials: credentials
      );

      var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

      _logger.LogInformation("JWT token generated successfully for user {UserId}", user.Id);

      return tokenString;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.Id);
      throw;
    }
  }
}
