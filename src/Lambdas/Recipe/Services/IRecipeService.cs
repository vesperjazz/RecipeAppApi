using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lambdas.Recipe.Services;

public interface IRecipeService
{
    Task<string> GetWelcomeMessageAsync();
    Task<IEnumerable<string>> GetRecipeNamesAsync();
}
