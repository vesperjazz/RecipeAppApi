using Microsoft.Extensions.Configuration;

namespace Lambdas.Recipe.Services;

public class RecipeService : IRecipeService
{
    private readonly IConfiguration _configuration;

    public RecipeService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> GetWelcomeMessageAsync()
    {
        // Simulate async operation
        await Task.Delay(1);
        return _configuration["AppSettings:WelcomeMessage"] ?? "Welcome to Recipe App API!";
    }

    public async Task<IEnumerable<string>> GetRecipeNamesAsync()
    {
        // Simulate async operation
        await Task.Delay(1);
        return new[] { "Spaghetti Carbonara", "Chicken Tikka Masala", "Caesar Salad", "Beef Stroganoff" };
    }
}
