using Xunit;
using BuildingBlocks.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace RecipeAppApi.Tests;

public class LoggingTests
{
    [Fact]
    public void LoggingConfiguration_ShouldConfigureSerilog_ForWebApplication()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Serilog:MinimumLevel:Default", "Information"},
                {"Serilog:WriteTo:0:Name", "Console"}
            })
            .Build();

        // Act & Assert - Should not throw
        var host = Host.CreateDefaultBuilder()
            .ConfigureSerilog(configuration)
            .Build();

        Assert.NotNull(host);
    }

    [Fact]
    public void LoggingConfiguration_ShouldConfigureSerilog_ForLambda()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Serilog:MinimumLevel:Default", "Information"},
                {"Serilog:WriteTo:0:Name", "Console"}
            })
            .Build();

        // Act & Assert - Should not throw
        var host = Host.CreateDefaultBuilder()
            .ConfigureSerilogForLambda(configuration)
            .Build();

        Assert.NotNull(host);
    }
}
