using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Core.Domain.Entities;
using Lambdas.Users.DTOs;
using Lambdas.Users.Services;

namespace Lambdas.Users.Services;

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserService> _logger;
    private readonly RecipeAppDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public UserService(
        IConfiguration configuration, 
        ILogger<UserService> logger,
        RecipeAppDbContext dbContext,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _logger.LogInformation("UserService initialized");
    }

    public async Task<string> GetWelcomeMessageAsync()
    {
        _logger.LogDebug("Getting welcome message from configuration");
        
        // Simulate async operation
        await Task.Delay(1);
        
        var message = _configuration["AppSettings:WelcomeMessage"] ?? "Welcome to Users App API!";
        _logger.LogInformation("Welcome message retrieved: {Message}", message);
        
        return message;
    }

    public async Task<IEnumerable<string>> GetUserNamesAsync()
    {
        _logger.LogDebug("Retrieving user names");
        
        // Simulate async operation
        await Task.Delay(1);
        
        var users = new[] { "John Doe", "Jane Smith", "Bob Johnson", "Alice Brown" };
        _logger.LogInformation("Retrieved {UserCount} users: {Users}", users.Length, string.Join(", ", users));
        
        return users;
    }

    public async Task<SignUpResponse> SignUpAsync(SignUpRequest request)
    {
        _logger.LogInformation("Starting sign up process for user: {Username}", request.Username);

        try
        {
            // Check if username already exists
            var existingUsername = await _dbContext.Users
                .AnyAsync(u => u.Username == request.Username);
            
            if (existingUsername)
            {
                _logger.LogWarning("Username already exists: {Username}", request.Username);
                throw new InvalidOperationException($"Username '{request.Username}' is already taken.");
            }

            // Check if email already exists
            var existingEmail = await _dbContext.Users
                .AnyAsync(u => u.Email == request.Email);
            
            if (existingEmail)
            {
                _logger.LogWarning("Email already exists: {Email}", request.Email);
                throw new InvalidOperationException($"Email '{request.Email}' is already registered.");
            }

            // Hash the password
            var (passwordHash, passwordSalt) = _passwordService.HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            // Add user to database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User successfully created with ID: {UserId}", user.Id);

            return new SignUpResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedDate = user.CreatedDate,
                Message = "User successfully registered."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during sign up for user: {Username}", request.Username);
            throw;
        }
    }

    public async Task<SignInResponse> SignInAsync(SignInRequest request)
    {
        _logger.LogInformation("Starting sign in process for user: {Username}", request.Username);

        try
        {
            // Find user by username
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);
            
            if (user == null)
            {
                _logger.LogWarning("User not found during sign in: {Username}", request.Username);
                throw new InvalidOperationException("Invalid username or password.");
            }

            // Validate password
            var isPasswordValid = _jwtService.ValidatePasswordAsync(
                request.Password, 
                user.PasswordHash, 
                user.PasswordSalt);
            
            if (!isPasswordValid)
            {
                _logger.LogWarning("Invalid password during sign in for user: {Username}", request.Username);
                throw new InvalidOperationException("Invalid username or password.");
            }

            // Get user roles (for now, default to "User" role)
            var roles = new[] { "User" };

            // Generate access token
            var accessToken = _jwtService.GenerateAccessTokenAsync(user, roles);

            // Get token expiration from configuration
            var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var exp) ? exp : 60;

            _logger.LogInformation("User successfully signed in: {UserId}", user.Id);

            return new SignInResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = expirationMinutes * 60, // Convert to seconds
                IssuedAt = DateTime.UtcNow,
                Message = "User successfully signed in."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during sign in for user: {Username}", request.Username);
            throw;
        }
    }
}
