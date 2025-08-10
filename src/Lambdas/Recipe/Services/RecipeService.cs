using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lambdas.Recipe.Services;

public class RecipeService : IRecipeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RecipeService> _logger;

    public RecipeService(IConfiguration configuration, ILogger<RecipeService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _logger.LogInformation("RecipeService initialized");
    }

    public async Task<string> GetWelcomeMessageAsync()
    {
        _logger.LogDebug("Getting welcome message from configuration");
        
        // Simulate async operation
        await Task.Delay(1);
        
        var message = _configuration["AppSettings:WelcomeMessage"] ?? "Welcome to Recipe App API!";
        _logger.LogInformation("Welcome message retrieved: {Message}", message);
        
        return message;
    }

    public async Task<IEnumerable<string>> GetRecipeNamesAsync()
    {
        _logger.LogDebug("Retrieving recipe names");
        
        // Simulate async operation
        await Task.Delay(1);
        
        var recipes = new[] { "Spaghetti Carbonara", "Chicken Tikka Masala", "Caesar Salad", "Beef Stroganoff" };
        _logger.LogInformation("Retrieved {RecipeCount} recipes: {Recipes}", recipes.Length, string.Join(", ", recipes));
        
        return recipes;
    }
}
