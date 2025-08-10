using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence;

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
            services.AddDbContext<RecipeAppDbContext>(options =>
            {
                options.UseInMemoryDatabase("RecipeAppInMemory");
            });
        }
        else
        {
            services.AddDbContext<RecipeAppDbContext>(options =>
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
