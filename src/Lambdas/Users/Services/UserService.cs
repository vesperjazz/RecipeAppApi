using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lambdas.Users.Services;

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserService> _logger;

    public UserService(IConfiguration configuration, ILogger<UserService> logger)
    {
        _configuration = configuration;
        _logger = logger;
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
}
