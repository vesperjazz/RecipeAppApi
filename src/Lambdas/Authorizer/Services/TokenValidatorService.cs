using Microsoft.Extensions.Configuration;

namespace Lambdas.Authorizer.Services;

public class TokenValidatorService : ITokenValidator
{
    private readonly IConfiguration _configuration;

    public TokenValidatorService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        // Simulate async operation
        await Task.Delay(1);
        
        // For now, return true for any non-empty token
        // This will be implemented with proper JWT validation later
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task<string> GetPrincipalIdAsync(string token)
    {
        // Simulate async operation
        await Task.Delay(1);
        
        // For now, return a default user ID
        // This will be implemented to extract user ID from JWT token later
        return "user";
    }
}
