using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using Infrastructure.Persistence;
using Core.Domain.Entities;
using Lambdas.Users.DTOs;
using Lambdas.Users.Services;

namespace RecipeAppApi.Tests;

public class UserServiceTests : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserService> _logger;
    private readonly RecipeAppDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<UserService>>();
        
        // Use InMemory database for testing
        var options = new DbContextOptionsBuilder<RecipeAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new RecipeAppDbContext(options);
        
        _passwordService = Substitute.For<IPasswordService>();
        
        _userService = new UserService(_configuration, _logger, _dbContext, _passwordService);
        
        // Ensure database is created
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GetWelcomeMessageAsync_WithConfiguredMessage_ReturnsConfiguredMessage()
    {
        // Arrange
        var expectedMessage = "Custom welcome message";
        _configuration["AppSettings:WelcomeMessage"].Returns(expectedMessage);

        // Act
        var result = await _userService.GetWelcomeMessageAsync();

        // Assert
        Assert.Equal(expectedMessage, result);
    }

    [Fact]
    public async Task GetWelcomeMessageAsync_WithoutConfiguredMessage_ReturnsDefaultMessage()
    {
        // Arrange
        _configuration["AppSettings:WelcomeMessage"].Returns((string)null);
        var expectedMessage = "Welcome to Users App API!";

        // Act
        var result = await _userService.GetWelcomeMessageAsync();

        // Assert
        Assert.Equal(expectedMessage, result);
    }

    [Fact]
    public async Task GetUserNamesAsync_ReturnsExpectedUserNames()
    {
        // Arrange
        var expectedUsers = new[] { "John Doe", "Jane Smith", "Bob Johnson", "Alice Brown" };

        // Act
        var result = await _userService.GetUserNamesAsync();

        // Assert
        Assert.Equal(expectedUsers, result);
    }

    [Fact]
    public async Task SignUpAsync_WithValidRequest_CreatesUserSuccessfully()
    {
        // Arrange
        var request = new SignUpRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123"
        };

        var passwordHash = "hashedPassword";
        var passwordSalt = "passwordSalt";

        _passwordService.HashPassword(request.Password).Returns((passwordHash, passwordSalt));

        // Act
        var result = await _userService.SignUpAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.Username, result.Username);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal("User successfully registered.", result.Message);
        
        // Verify user was actually saved to database
        var savedUser = await _dbContext.Users.FindAsync(result.Id);
        Assert.NotNull(savedUser);
        Assert.Equal(request.Username, savedUser.Username);
        Assert.Equal(request.Email, savedUser.Email);
        Assert.Equal(passwordHash, savedUser.PasswordHash);
        Assert.Equal(passwordSalt, savedUser.PasswordSalt);
    }

    [Fact]
    public async Task SignUpAsync_WithExistingUsername_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
        
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var request = new SignUpRequest
        {
            Username = "existinguser",
            Email = "test@example.com",
            Password = "password123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.SignUpAsync(request));
        
        Assert.Equal("Username 'existinguser' is already taken.", exception.Message);
        
        // Verify no additional users were added
        var userCount = await _dbContext.Users.CountAsync();
        Assert.Equal(1, userCount);
    }

    [Fact]
    public async Task SignUpAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
        
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var request = new SignUpRequest
        {
            Username = "newuser",
            Email = "existing@example.com",
            Password = "password123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.SignUpAsync(request));
        
        Assert.Equal("Email 'existing@example.com' is already registered.", exception.Message);
        
        // Verify no additional users were added
        var userCount = await _dbContext.Users.CountAsync();
        Assert.Equal(1, userCount);
    }

    [Fact]
    public async Task SignUpAsync_WhenDatabaseExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var request = new SignUpRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123"
        };

        _passwordService.HashPassword(request.Password).Returns(("hash", "salt"));
        
        // Create a new context that will be disposed to simulate database failure
        var options = new DbContextOptionsBuilder<RecipeAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var failingContext = new RecipeAppDbContext(options);
        var failingUserService = new UserService(_configuration, _logger, failingContext, _passwordService);
        
        // Dispose the context to simulate a database connection failure
        failingContext.Dispose();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ObjectDisposedException>(
            () => failingUserService.SignUpAsync(request));
        
        Assert.Contains("RecipeAppDbContext", exception.Message);
        
        failingContext.Dispose();
    }

    [Fact]
    public async Task SignUpAsync_WithValidRequest_SetsCorrectUserProperties()
    {
        // Arrange
        var request = new SignUpRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123"
        };

        var passwordHash = "hashedPassword";
        var passwordSalt = "passwordSalt";

        _passwordService.HashPassword(request.Password).Returns((passwordHash, passwordSalt));

        // Act
        await _userService.SignUpAsync(request);

        // Assert
        var savedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        Assert.NotNull(savedUser);
        Assert.NotEqual(Guid.Empty, savedUser.Id);
        Assert.Equal(request.Username, savedUser.Username);
        Assert.Equal(request.Email, savedUser.Email);
        Assert.Equal(passwordHash, savedUser.PasswordHash);
        Assert.Equal(passwordSalt, savedUser.PasswordSalt);
        Assert.True(savedUser.CreatedDate <= DateTime.UtcNow);
        Assert.True(savedUser.UpdatedDate <= DateTime.UtcNow);
    }
}
