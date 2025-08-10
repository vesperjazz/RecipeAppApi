using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Lambdas.Authorizer.Services;
using BuildingBlocks.Observability;

namespace Lambdas.Authorizer;

public class Function
{
    private readonly IConfiguration _configuration;
    private readonly ITokenValidator _tokenValidator;
    private readonly ILogger<Function> _logger;

    public Function()
    {
        // Configure configuration to load from appsettings.json and environment variables
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(context.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables(); // Environment variables override JSON settings
            })
            .ConfigureServices((context, services) =>
            {
                // Register services with dependency injection
                services.AddScoped<ITokenValidator, TokenValidatorService>();
                
                // Add database context
                services.AddAuthorizerDatabase(context.Configuration);
            })
            .Build();

        _configuration = host.Services.GetRequiredService<IConfiguration>();
        
        // Configure Serilog after getting the configuration
        var hostWithLogging = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(context.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureSerilogForLambda(_configuration)
            .ConfigureServices((context, services) =>
            {
                services.AddScoped<ITokenValidator, TokenValidatorService>();
                
                // Add database context
                services.AddAuthorizerDatabase(context.Configuration);
            })
            .Build();

        _tokenValidator = hostWithLogging.Services.GetRequiredService<ITokenValidator>();
        _logger = hostWithLogging.Services.GetRequiredService<ILogger<Function>>();
        
        _logger.LogInformation("Function initialized with configuration loaded");
    }

    public async Task<APIGatewayCustomAuthorizerResponse> FunctionHandler(APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
    {
        _logger.LogInformation("Processing authorization request for request ID: {RequestId}", context.AwsRequestId);
        
        // Get configuration values
        var issuer = _configuration["Authorizer:Issuer"] ?? "RecipeAppApi";
        var audience = _configuration["Authorizer:Audience"] ?? "RecipeAppApi";
        
        _logger.LogDebug("Using issuer: {Issuer}, audience: {Audience}", issuer, audience);

        // Extract token from the request
        var token = ExtractTokenFromRequest(request);
        _logger.LogDebug("Token extracted from request, length: {TokenLength}", token.Length);
        
        // Validate token using injected service
        var isValid = await _tokenValidator.ValidateTokenAsync(token);
        var principalId = await _tokenValidator.GetPrincipalIdAsync(token);
        
        _logger.LogInformation("Token validation result: {IsValid}, Principal ID: {PrincipalId}", isValid, principalId);

        if (!isValid)
        {
            _logger.LogWarning("Token validation failed for request ID: {RequestId}", context.AwsRequestId);
            
            // Return deny policy for invalid tokens
            var denyPolicy = new APIGatewayCustomAuthorizerPolicy
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                {
                    new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                    {
                        Effect = "Deny",
                        Action = new HashSet<string> { "execute-api:Invoke" },
                        Resource = new HashSet<string> { "*" }
                    }
                }
            };

            return new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = principalId,
                PolicyDocument = denyPolicy
            };
        }

        _logger.LogInformation("Token validation successful for request ID: {RequestId}", context.AwsRequestId);
        
        // Return allow policy for valid tokens
        var allowPolicy = new APIGatewayCustomAuthorizerPolicy
        {
            Version = "2012-10-17",
            Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
            {
                new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                {
                    Effect = "Allow",
                    Action = new HashSet<string> { "execute-api:Invoke" },
                    Resource = new HashSet<string> { "*" }
                }
            }
        };

        return new APIGatewayCustomAuthorizerResponse
        {
            PrincipalID = principalId,
            PolicyDocument = allowPolicy
        };
    }

    private string ExtractTokenFromRequest(APIGatewayCustomAuthorizerRequest request)
    {
        // Extract token from Authorization header
        if (request.Headers != null && request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Token extracted from Authorization header");
                return authHeader.Substring("Bearer ".Length);
            }
        }

        // Fallback to query string parameter
        if (request.QueryStringParameters != null && request.QueryStringParameters.TryGetValue("token", out var tokenParam))
        {
            _logger.LogDebug("Token extracted from query string parameter");
            return tokenParam;
        }

        _logger.LogWarning("No token found in request");
        return string.Empty;
    }
}
