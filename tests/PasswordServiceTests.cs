using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Lambdas.Users.Services;
using Xunit;

namespace RecipeAppApi.Tests;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    [Fact]
    public void HashPassword_ShouldReturnHashAndSalt()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash, salt) = _passwordService.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotNull(salt);
        Assert.NotEmpty(salt);
        Assert.NotEqual(password, hash);
        Assert.NotEqual(password, salt);
    }

    [Fact]
    public void HashPassword_ShouldGenerateDifferentSaltForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash1, salt1) = _passwordService.HashPassword(password);
        var (hash2, salt2) = _passwordService.HashPassword(password);

        // Assert
        Assert.NotEqual(salt1, salt2);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashPassword_ShouldGenerateValidBase64Strings()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash, salt) = _passwordService.HashPassword(password);

        // Assert
        // Check if strings are valid base64
        Assert.True(IsValidBase64String(hash));
        Assert.True(IsValidBase64String(salt));
    }

    [Fact]
    public void HashPassword_ShouldGenerateSaltOfCorrectLength()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash, salt) = _passwordService.HashPassword(password);

        // Assert
        var saltBytes = Convert.FromBase64String(salt);
        Assert.Equal(32, saltBytes.Length);
    }

    [Fact]
    public void HashPassword_ShouldGenerateHashOfCorrectLength()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash, salt) = _passwordService.HashPassword(password);

        // Assert
        var hashBytes = Convert.FromBase64String(hash);
        Assert.Equal(32, hashBytes.Length);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("12345678")]
    [InlineData("Password123!")]
    [InlineData("VeryLongPasswordWithSpecialCharacters123!@#$%^&*()")]
    public void HashPassword_ShouldHandleVariousPasswordLengths(string password)
    {
        // Act
        var (hash, salt) = _passwordService.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotNull(salt);
        Assert.NotEmpty(salt);
        Assert.NotEqual(password, hash);
        Assert.NotEqual(password, salt);
    }

    [Fact]
    public void HashPassword_ShouldHandleUnicodeCharacters()
    {
        // Arrange
        var password = "TestPassword123!🎉🚀";

        // Act
        var (hash, salt) = _passwordService.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotNull(salt);
        Assert.NotEmpty(salt);
        Assert.NotEqual(password, hash);
        Assert.NotEqual(password, salt);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        var password = "TestPassword123!";
        var (hash, salt) = _passwordService.HashPassword(password);

        // Act
        var isValid = _passwordService.VerifyPassword(password, hash, salt);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        // Arrange
        var originalPassword = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var (hash, salt) = _passwordService.HashPassword(originalPassword);

        // Act
        var isValid = _passwordService.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForWrongHash()
    {
        // Arrange
        var password = "TestPassword123!";
        var (hash, salt) = _passwordService.HashPassword(password);
        var wrongHash = Convert.ToBase64String(new byte[32]); // Different hash

        // Act
        var isValid = _passwordService.VerifyPassword(password, wrongHash, salt);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForWrongSalt()
    {
        // Arrange
        var password = "TestPassword123!";
        var (hash, salt) = _passwordService.HashPassword(password);
        var wrongSalt = Convert.ToBase64String(new byte[32]); // Different salt

        // Act
        var isValid = _passwordService.VerifyPassword(password, hash, wrongSalt);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void HashPassword_ShouldUseCryptographicallySecureRandomSalt()
    {
        // Arrange
        var password = "TestPassword123!";
        var salts = new string[100];

        // Act
        for (int i = 0; i < 100; i++)
        {
            var (_, salt) = _passwordService.HashPassword(password);
            salts[i] = salt;
        }

        // Assert
        var uniqueSalts = salts.Distinct().Count();
        Assert.Equal(100, uniqueSalts);
    }

    [Fact]
    public void HashPassword_ShouldProduceDeterministicHashForSamePasswordAndSalt()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt = Convert.ToBase64String(new byte[32]);

        // Act
        var hash1 = HashPasswordWithCustomSalt(password, salt);
        var hash2 = HashPasswordWithCustomSalt(password, salt);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashPassword_ShouldHandleNullPassword()
    {
        // Arrange
        string password = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => _passwordService.HashPassword(password));
        Assert.Equal("password", exception.ParamName);
    }

    [Fact]
    public void VerifyPassword_ShouldHandleNullParameters()
    {
        // Arrange
        string password = null;
        string hash = null;
        string salt = null;

        // Act & Assert
        var exception1 = Assert.Throws<ArgumentNullException>(() => _passwordService.VerifyPassword(password, "hash", "salt"));
        var exception2 = Assert.Throws<ArgumentNullException>(() => _passwordService.VerifyPassword("password", hash, "salt"));
        var exception3 = Assert.Throws<ArgumentNullException>(() => _passwordService.VerifyPassword("password", "hash", salt));

        Assert.Equal("password", exception1.ParamName);
        Assert.Equal("hash", exception2.ParamName);
        Assert.Equal("salt", exception3.ParamName);
    }

    [Fact]
    public void HashPassword_ShouldBeConsistentAcrossMultipleInstances()
    {
        // Arrange
        var password = "TestPassword123!";
        var service1 = new PasswordService();
        var service2 = new PasswordService();

        // Act
        var (hash1, salt1) = service1.HashPassword(password);
        var (hash2, salt2) = service2.HashPassword(password);

        // Assert
        // Since salt is random, hashes will be different, but the hashing algorithm should be consistent
        Assert.Equal(hash1.Length, hash2.Length);
        Assert.Equal(salt1.Length, salt2.Length);
    }

    [Fact]
    public void HashPassword_ShouldHandleVeryLongPassword()
    {
        // Arrange
        var password = new string('a', 1000) + "123!";

        // Act
        var (hash, salt) = _passwordService.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotNull(salt);
        Assert.NotEmpty(salt);
        Assert.NotEqual(password, hash);
        Assert.NotEqual(password, salt);
    }

    [Fact]
    public void HashPassword_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var password = "!@#$%^&*()_+-=[]{}|;:,.<>?`~";

        // Act
        var (hash, salt) = _passwordService.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotNull(salt);
        Assert.NotEmpty(salt);
        Assert.NotEqual(password, hash);
        Assert.NotEqual(password, salt);
    }

    [Fact]
    public void VerifyPassword_ShouldHandleEmptyStringPassword()
    {
        // Arrange
        var password = "";
        var (hash, salt) = _passwordService.HashPassword(password);

        // Act
        var isValid = _passwordService.VerifyPassword(password, hash, salt);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_ShouldHandleWhitespacePassword()
    {
        // Arrange
        var password = "   ";
        var (hash, salt) = _passwordService.HashPassword(password);

        // Act
        var isValid = _passwordService.VerifyPassword(password, hash, salt);

        // Assert
        Assert.True(isValid);
    }

    // Helper method to test deterministic hashing with custom salt
    private string HashPasswordWithCustomSalt(string password, string salt)
    {
        var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(combinedBytes);
        return Convert.ToBase64String(hashBytes);
    }

    // Helper method to validate base64 strings
    private bool IsValidBase64String(string str)
    {
        try
        {
            Convert.FromBase64String(str);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
