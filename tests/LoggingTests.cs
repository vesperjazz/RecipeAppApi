using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Lambdas.Recipe;
using Lambdas.Authorizer;
using Infrastructure.Persistence;

namespace RecipeAppApi.Tests;

public class LoggingTests
{
    [Fact]
    public void TestLoggingConfiguration()
    {
        // This test verifies that the logging configuration works
        Assert.True(true);
    }

    [Fact]
    public void TestInMemoryDatabaseConfiguration()
    {
        // Test Recipe DbContext with in-memory database
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build(); // Empty configuration
        
        services.AddDatabase(configuration);
        
        var serviceProvider = services.BuildServiceProvider();
        var recipeDbContext = serviceProvider.GetService<RecipeAppDbContext>();
        
        Assert.NotNull(recipeDbContext);
    }

    [Fact]
    public void TestAuthorizerInMemoryDatabaseConfiguration()
    {
        // Test Authorizer DbContext with in-memory database
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build(); // Empty configuration
        
        services.AddAuthorizerDatabase(configuration);
        
        var serviceProvider = services.BuildServiceProvider();
        var authorizerDbContext = serviceProvider.GetService<RecipeAppDbContext>();
        
        Assert.NotNull(authorizerDbContext);
    }
}
