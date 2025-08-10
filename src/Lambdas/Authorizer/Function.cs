using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lambdas.Authorizer;

public class Function
{
    private readonly IConfiguration _configuration;

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
            .Build();

        _configuration = host.Services.GetRequiredService<IConfiguration>();
    }

    public async Task<APIGatewayCustomAuthorizerResponse> FunctionHandler(APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
    {
        // Get configuration values
        var issuer = _configuration["Authorizer:Issuer"] ?? "RecipeAppApi";
        var audience = _configuration["Authorizer:Audience"] ?? "RecipeAppApi";

        // For now, return a basic allow policy
        // This will be implemented with proper JWT validation later
        var policy = new APIGatewayCustomAuthorizerPolicy
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
            PrincipalID = "user",
            PolicyDocument = policy
        };
    }
}
