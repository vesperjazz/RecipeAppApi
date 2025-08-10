using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdas.Authorizer;

public static class AuthorizerDatabaseConfiguration
{
    public static IServiceCollection AddAuthorizerDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AuthorizerConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback to default connection if authorizer-specific connection is not configured
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Use in-memory database when no connection string is provided
            services.AddDbContext<AuthorizerDbContext>(options =>
            {
                options.UseInMemoryDatabase("AuthorizerAppInMemory");
            });
        }
        else
        {
            services.AddDbContext<AuthorizerDbContext>(options =>
            {
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null));
            });
        }

        return services;
    }
}
