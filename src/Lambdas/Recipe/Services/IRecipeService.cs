namespace Lambdas.Recipe.Services;

public interface IRecipeService
{
    Task<string> GetWelcomeMessageAsync();
    Task<IEnumerable<string>> GetRecipeNamesAsync();
}
