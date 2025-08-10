using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Lambdas.Authorizer.Services;

namespace Lambdas.Authorizer;

public class Function
{
    private readonly IConfiguration _configuration;
    private readonly ITokenValidator _tokenValidator;

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
            })
            .Build();

        _configuration = host.Services.GetRequiredService<IConfiguration>();
        _tokenValidator = host.Services.GetRequiredService<ITokenValidator>();
    }

    public async Task<APIGatewayCustomAuthorizerResponse> FunctionHandler(APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
    {
        // Get configuration values
        var issuer = _configuration["Authorizer:Issuer"] ?? "RecipeAppApi";
        var audience = _configuration["Authorizer:Audience"] ?? "RecipeAppApi";

        // Extract token from the request
        var token = ExtractTokenFromRequest(request);
        
        // Validate token using injected service
        var isValid = await _tokenValidator.ValidateTokenAsync(token);
        var principalId = await _tokenValidator.GetPrincipalIdAsync(token);

        if (!isValid)
        {
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
                return authHeader.Substring("Bearer ".Length);
            }
        }

        // Fallback to query string parameter
        if (request.QueryStringParameters != null && request.QueryStringParameters.TryGetValue("token", out var tokenParam))
        {
            return tokenParam;
        }

        return string.Empty;
    }
}
