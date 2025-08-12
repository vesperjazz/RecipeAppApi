using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lambdas.Recipe.DTOs;

namespace Lambdas.Recipe.Services;

public interface IRecipeService
{
    Task<string> GetWelcomeMessageAsync();
    Task<IEnumerable<string>> GetRecipeNamesAsync();
    Task<CreateRecipeResponse> CreateRecipeAsync(CreateRecipeRequest request, Guid createdByUserId);
    Task<SearchRecipeResponse> SearchRecipesAsync(SearchRecipeRequest request);
}
