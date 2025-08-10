using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lambdas.Authorizer.Services;

public class TokenValidatorService : ITokenValidator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenValidatorService> _logger;

    public TokenValidatorService(IConfiguration configuration, ILogger<TokenValidatorService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _logger.LogInformation("TokenValidatorService initialized");
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        _logger.LogDebug("Validating token, length: {TokenLength}", token?.Length ?? 0);
        
        // Simulate async operation
        await Task.Delay(1);
        
        // For now, return true for any non-empty token
        // This will be implemented with proper JWT validation later
        var isValid = !string.IsNullOrWhiteSpace(token);
        _logger.LogInformation("Token validation completed: {IsValid}", isValid);
        
        return isValid;
    }

    public async Task<string> GetPrincipalIdAsync(string token)
    {
        _logger.LogDebug("Extracting principal ID from token, length: {TokenLength}", token?.Length ?? 0);
        
        // Simulate async operation
        await Task.Delay(1);
        
        // For now, return a default user ID
        // This will be implemented to extract user ID from JWT token later
        var principalId = "user";
        _logger.LogInformation("Principal ID extracted: {PrincipalId}", principalId);
        
        return principalId;
    }
}
