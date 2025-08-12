using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence;

namespace Lambdas.Recipe;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
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
