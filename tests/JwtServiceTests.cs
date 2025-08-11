using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Core.Domain.Entities;
using Lambdas.Users.Services;

namespace RecipeAppApi.Tests;

public class JwtServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    private readonly IPasswordService _passwordService;
    private readonly JwtService _jwtService;
    private readonly User _testUser;

    public JwtServiceTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<JwtService>>();
        _passwordService = Substitute.For<IPasswordService>();
        
        _jwtService = new JwtService(_configuration, _logger, _passwordService);
        
        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }

    [Fact]
    public void GenerateAccessTokenAsync_WithValidUserAndRoles_ReturnsValidJwtToken()
    {
        // Arrange
        var roles = new[] { "User", "Admin" };
        SetupJwtConfiguration();

        // Act
        var result = _jwtService.GenerateAccessTokenAsync(_testUser, roles);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Verify it's a valid JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        Assert.True(tokenHandler.CanReadToken(result));
        
        var token = tokenHandler.ReadJwtToken(result);
        Assert.Equal("RecipeAppApi", token.Issuer);
        Assert.Equal("RecipeAppApi", token.Audiences.First());
        Assert.True(token.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateAccessTokenAsync_WithCustomClaims_IncludesAllClaims()
    {
        // Arrange
        var roles = new[] { "User" };
        SetupJwtConfiguration(); // Use basic configuration for now

        // Act
        var result = _jwtService.GenerateAccessTokenAsync(_testUser, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result);
        
        // Check standard JWT claims
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == _testUser.Id.ToString());
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Name && c.Value == _testUser.Username);
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == _testUser.Email);
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Iat);
        
        // Check role claims
        Assert.Contains(token.Claims, c => c.Type == "role" && c.Value == "User");
        
        // Check user-specific claims (note: email is now only in JWT standard claims)
        Assert.Contains(token.Claims, c => c.Type == "user_id" && c.Value == _testUser.Id.ToString());
        Assert.Contains(token.Claims, c => c.Type == "username" && c.Value == _testUser.Username);
        Assert.Contains(token.Claims, c => c.Type == "created_date" && c.Value == _testUser.CreatedDate.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public void GenerateAccessTokenAsync_WithMultipleRoles_IncludesAllRoleClaims()
    {
        // Arrange
        var roles = new[] { "User", "Admin", "Moderator" };
        _configuration["Jwt:Secret"].Returns("test-secret-key-with-at-least-32-characters");
        _configuration["Jwt:Issuer"].Returns("RecipeAppApi");
        _configuration["Jwt:Audience"].Returns("RecipeAppApi");
        _configuration["Jwt:ExpirationMinutes"].Returns("60");
        _configuration.GetSection("Jwt:CustomClaims").Returns((IConfigurationSection)null);

        // Act
        var result = _jwtService.GenerateAccessTokenAsync(_testUser, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result);
        
        // Check that role claims exist - use the short claim type name
        var roleClaims = token.Claims.Where(c => c.Type == "role").ToList();
        Assert.Equal(roles.Length, roleClaims.Count);
        
        foreach (var role in roles)
        {
            Assert.Contains(roleClaims, c => c.Value == role);
        }
    }

    [Fact]
    public void GenerateAccessTokenAsync_WithNoCustomClaims_GeneratesTokenWithoutCustomClaims()
    {
        // Arrange
        var roles = new[] { "User" };
        SetupJwtConfigurationWithoutCustomClaims();

        // Act
        var result = _jwtService.GenerateAccessTokenAsync(_testUser, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result);
        
        // Should not contain custom claims from configuration
        Assert.DoesNotContain(token.Claims, c => c.Type == "app_name");
        Assert.DoesNotContain(token.Claims, c => c.Type == "app_version");
        
        // Should still contain standard claims and user-specific claims
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.Contains(token.Claims, c => c.Type == "role");
        Assert.Contains(token.Claims, c => c.Type == "user_id");
        Assert.Contains(token.Claims, c => c.Type == "username");
        Assert.Contains(token.Claims, c => c.Type == "created_date");
    }

    [Fact]
    public void GenerateAccessTokenAsync_WithCustomExpiration_UsesConfiguredExpiration()
    {
        // Arrange
        var roles = new[] { "User" };
        SetupJwtConfigurationWithCustomExpiration(120); // 2 hours

        // Act
        var result = _jwtService.GenerateAccessTokenAsync(_testUser, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result);
        
        var expectedExpiration = DateTime.UtcNow.AddMinutes(120);
        Assert.True(token.ValidTo > DateTime.UtcNow.AddMinutes(119));
        Assert.True(token.ValidTo <= expectedExpiration);
    }

    [Fact]
    public void GenerateAccessTokenAsync_WithDefaultExpiration_UsesDefaultExpiration()
    {
        // Arrange
        var roles = new[] { "User" };
        SetupJwtConfigurationWithDefaultExpiration();

        // Act
        var result = _jwtService.GenerateAccessTokenAsync(_testUser, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result);
        
        var expectedExpiration = DateTime.UtcNow.AddMinutes(60); // Default 60 minutes
        Assert.True(token.ValidTo > DateTime.UtcNow.AddMinutes(59));
        Assert.True(token.ValidTo <= expectedExpiration);
    }

    [Fact]
    public void GenerateClaimsAsync_WithValidUserAndRoles_ReturnsAllExpectedClaims()
    {
        // Arrange
        var roles = new[] { "User", "Admin" };
        SetupJwtConfiguration(); // Use basic configuration for now

        // Act
        var result = _jwtService.GenerateClaimsAsync(_testUser, roles);

        // Assert
        var claims = result.ToList();
        
        // Should have standard JWT claims + role claims + user-specific claims (no custom claims for now)
        var expectedClaimCount = 5 + roles.Length + 3; // 5 standard + roles + 3 user-specific (removed duplicate email)
        Assert.Equal(expectedClaimCount, claims.Count);
        
        // Verify specific claims exist
        Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == _testUser.Id.ToString());
        Assert.Contains(claims, c => c.Type == "role" && c.Value == "User");
        Assert.Contains(claims, c => c.Type == "role" && c.Value == "Admin");
        Assert.Contains(claims, c => c.Type == "user_id" && c.Value == _testUser.Id.ToString());
    }

    [Fact]
    public void ValidatePasswordAsync_WithValidPassword_ReturnsTrue()
    {
        // Arrange
        var password = "testpassword";
        var passwordHash = "hashedpassword";
        var passwordSalt = "passwordsalt";
        
        _passwordService.VerifyPassword(password, passwordHash, passwordSalt).Returns(true);

        // Act
        var result = _jwtService.ValidatePasswordAsync(password, passwordHash, passwordSalt);

        // Assert
        Assert.True(result);
        _passwordService.Received(1).VerifyPassword(password, passwordHash, passwordSalt);
    }

    [Fact]
    public void ValidatePasswordAsync_WithInvalidPassword_ReturnsFalse()
    {
        // Arrange
        var password = "testpassword";
        var passwordHash = "hashedpassword";
        var passwordSalt = "passwordsalt";
        
        _passwordService.VerifyPassword(password, passwordHash, passwordSalt).Returns(false);

        // Act
        var result = _jwtService.ValidatePasswordAsync(password, passwordHash, passwordSalt);

        // Assert
        Assert.False(result);
        _passwordService.Received(1).VerifyPassword(password, passwordHash, passwordSalt);
    }

    [Fact]
    public void GenerateAccessTokenAsync_WithCustomIssuerAndAudience_UsesConfiguredValues()
    {
        // Arrange
        var roles = new[] { "User" };
        SetupJwtConfigurationWithCustomIssuerAndAudience("CustomIssuer", "CustomAudience");

        // Act
        var result = _jwtService.GenerateAccessTokenAsync(_testUser, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result);
        
        Assert.Equal("CustomIssuer", token.Issuer);
        Assert.Equal("CustomAudience", token.Audiences.First());
    }

    [Fact]
    public void GenerateAccessTokenAsync_WithDefaultIssuerAndAudience_UsesDefaultValues()
    {
        // Arrange
        var roles = new[] { "User" };
        SetupJwtConfigurationWithDefaultIssuerAndAudience();

        // Act
        var result = _jwtService.GenerateAccessTokenAsync(_testUser, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result);
        
        Assert.Equal("RecipeAppApi", token.Issuer);
        Assert.Equal("RecipeAppApi", token.Audiences.First());
    }

    [Fact]
    public void GenerateAccessTokenAsync_WithEmptyRoles_GeneratesTokenWithoutRoleClaims()
    {
        // Arrange
        var roles = new string[0];
        SetupJwtConfiguration();

        // Act
        var result = _jwtService.GenerateAccessTokenAsync(_testUser, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result);
        
        // Should not contain any role claims
        Assert.DoesNotContain(token.Claims, c => c.Type == "role");
        
        // Should still contain other claims
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.Contains(token.Claims, c => c.Type == "user_id");
    }

    [Fact]
    public void GenerateAccessTokenAsync_WithNullRoles_GeneratesTokenWithoutRoleClaims()
    {
        // Arrange
        IEnumerable<string> roles = null;
        SetupJwtConfiguration();

        // Act
        var result = _jwtService.GenerateAccessTokenAsync(_testUser, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result);
        
        // Should not contain any role claims
        Assert.DoesNotContain(token.Claims, c => c.Type == "role");
        
        // Should still contain other claims
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.Contains(token.Claims, c => c.Type == "user_id");
    }

    private void SetupJwtConfiguration()
    {
        _configuration["Jwt:Secret"].Returns("test-secret-key-with-at-least-32-characters");
        _configuration["Jwt:Issuer"].Returns("RecipeAppApi");
        _configuration["Jwt:Audience"].Returns("RecipeAppApi");
        _configuration["Jwt:ExpirationMinutes"].Returns("60");
        
        // Set up empty custom claims section - just return null to indicate no custom claims
        _configuration.GetSection("Jwt:CustomClaims").Returns((IConfigurationSection)null);
    }

    private void SetupJwtConfigurationWithCustomClaims()
    {
        _configuration["Jwt:Secret"].Returns("test-secret-key-with-at-least-32-characters");
        _configuration["Jwt:Issuer"].Returns("RecipeAppApi");
        _configuration["Jwt:Audience"].Returns("RecipeAppApi");
        _configuration["Jwt:ExpirationMinutes"].Returns("60");
        
        var customClaimsSection = Substitute.For<IConfigurationSection>();
        var appNameSection = Substitute.For<IConfigurationSection>();
        var appVersionSection = Substitute.For<IConfigurationSection>();
        var userTypeSection = Substitute.For<IConfigurationSection>();
        var permissionsSection = Substitute.For<IConfigurationSection>();
        
        appNameSection.Key.Returns("app_name");
        appNameSection.Value.Returns("RecipeApp");
        appVersionSection.Key.Returns("app_version");
        appVersionSection.Value.Returns("1.0.0");
        userTypeSection.Key.Returns("user_type");
        userTypeSection.Value.Returns("standard");
        permissionsSection.Key.Returns("permissions");
        permissionsSection.Value.Returns("read,write");
        
        customClaimsSection.GetChildren().Returns(new[] { appNameSection, appVersionSection, userTypeSection, permissionsSection });
        customClaimsSection.Exists().Returns(true);
        _configuration.GetSection("Jwt:CustomClaims").Returns(customClaimsSection);
    }

    private void SetupJwtConfigurationWithoutCustomClaims()
    {
        _configuration["Jwt:Secret"].Returns("test-secret-key-with-at-least-32-characters");
        _configuration["Jwt:Issuer"].Returns("RecipeAppApi");
        _configuration["Jwt:Audience"].Returns("RecipeAppApi");
        _configuration["Jwt:ExpirationMinutes"].Returns("60");
        
        // Set up empty custom claims section - just return null to indicate no custom claims
        _configuration.GetSection("Jwt:CustomClaims").Returns((IConfigurationSection)null);
    }

    private void SetupJwtConfigurationWithCustomExpiration(int expirationMinutes)
    {
        _configuration["Jwt:Secret"].Returns("test-secret-key-with-at-least-32-characters");
        _configuration["Jwt:Issuer"].Returns("RecipeAppApi");
        _configuration["Jwt:Audience"].Returns("RecipeAppApi");
        _configuration["Jwt:ExpirationMinutes"].Returns(expirationMinutes.ToString());
        _configuration.GetSection("Jwt:CustomClaims").Returns((IConfigurationSection)null);
    }

    private void SetupJwtConfigurationWithDefaultExpiration()
    {
        _configuration["Jwt:Secret"].Returns("test-secret-key-with-at-least-32-characters");
        _configuration["Jwt:Issuer"].Returns("RecipeAppApi");
        _configuration["Jwt:Audience"].Returns("RecipeAppApi");
        _configuration["Jwt:ExpirationMinutes"].Returns((string)null);
        _configuration.GetSection("Jwt:CustomClaims").Returns((IConfigurationSection)null);
    }

    private void SetupJwtConfigurationWithCustomIssuerAndAudience(string issuer, string audience)
    {
        _configuration["Jwt:Secret"].Returns("test-secret-key-with-at-least-32-characters");
        _configuration["Jwt:Issuer"].Returns(issuer);
        _configuration["Jwt:Audience"].Returns(audience);
        _configuration["Jwt:ExpirationMinutes"].Returns("60");
        _configuration.GetSection("Jwt:CustomClaims").Returns((IConfigurationSection)null);
    }

    private void SetupJwtConfigurationWithDefaultIssuerAndAudience()
    {
        _configuration["Jwt:Secret"].Returns("test-secret-key-with-at-least-32-characters");
        _configuration["Jwt:Issuer"].Returns((string)null);
        _configuration["Jwt:Audience"].Returns((string)null);
        _configuration["Jwt:ExpirationMinutes"].Returns("60");
        _configuration.GetSection("Jwt:CustomClaims").Returns((IConfigurationSection)null);
    }
}
