using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Core.Domain.Entities;
using Lambdas.Users.Services;

namespace Lambdas.Users.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    private readonly IPasswordService _passwordService;

    public JwtService(
        IConfiguration configuration,
        ILogger<JwtService> logger,
        IPasswordService passwordService)
    {
        _configuration = configuration;
        _logger = logger;
        _passwordService = passwordService;
        _logger.LogInformation("JwtService initialized");
    }

    public string GenerateAccessTokenAsync(User user, IEnumerable<string> roles)
    {
        _logger.LogDebug("Generating access token for user: {UserId}", user.Id);

        var claims = GenerateClaimsAsync(user, roles);
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtSecret()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = GetJwtIssuer(),
            Audience = GetJwtAudience(),
            Expires = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation("Access token generated successfully for user: {UserId}", user.Id);
        return tokenString;
    }

    public IEnumerable<Claim> GenerateClaimsAsync(User user, IEnumerable<string> roles)
    {
        _logger.LogDebug("Generating claims for user: {UserId}", user.Id);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add custom claims from configuration
        var customClaims = GetCustomClaims();
        foreach (var claim in customClaims)
        {
            claims.Add(new Claim(claim.Key, claim.Value));
        }

        // Add role claims (handle null/empty roles)
        if (roles != null)
        {
            foreach (var role in roles)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    claims.Add(new Claim("role", role));
                }
            }
        }

        // Add user-specific custom claims (avoiding duplicates with JWT standard claims)
        claims.Add(new Claim("user_id", user.Id.ToString()));
        claims.Add(new Claim("username", user.Username));
        // Note: email is already included in JWT standard claims above
        claims.Add(new Claim("created_date", user.CreatedDate.ToString("yyyy-MM-dd")));

        _logger.LogDebug("Generated {ClaimCount} claims for user: {UserId}", claims.Count, user.Id);
        return claims;
    }

    public bool ValidatePasswordAsync(string password, string passwordHash, string passwordSalt)
    {
        _logger.LogDebug("Validating password for user");
        
        var isValid = _passwordService.VerifyPassword(password, passwordHash, passwordSalt);
        
        _logger.LogDebug("Password validation result: {IsValid}", isValid);
        return isValid;
    }

    private string GetJwtSecret()
    {
        var secret = _configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(secret))
        {
            _logger.LogWarning("JWT secret not configured, using default secret");
            secret = "your-super-secret-key-with-at-least-32-characters";
        }
        return secret;
    }

    private string GetJwtIssuer()
    {
        return _configuration["Jwt:Issuer"] ?? "RecipeAppApi";
    }

    private string GetJwtAudience()
    {
        return _configuration["Jwt:Audience"] ?? "RecipeAppApi";
    }

    private int GetJwtExpirationMinutes()
    {
        if (int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var expiration))
        {
            return expiration;
        }
        return 60; // Default to 60 minutes
    }

    private Dictionary<string, string> GetCustomClaims()
    {
        var customClaims = new Dictionary<string, string>();
        
        var customClaimsSection = _configuration.GetSection("Jwt:CustomClaims");
        if (customClaimsSection.Exists())
        {
            foreach (var claim in customClaimsSection.GetChildren())
            {
                customClaims[claim.Key] = claim.Value ?? string.Empty;
            }
        }

        return customClaims;
    }
}
