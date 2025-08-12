using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Core.Domain.Entities;
using Lambdas.Recipe.DTOs;

namespace Lambdas.Recipe.Services;

public class RecipeService : IRecipeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RecipeService> _logger;
    private readonly RecipeAppDbContext _dbContext;

    public RecipeService(IConfiguration configuration, ILogger<RecipeService> logger, RecipeAppDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
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

    public async Task<CreateRecipeResponse> CreateRecipeAsync(CreateRecipeRequest request, Guid createdByUserId)
    {
        _logger.LogInformation("Creating new recipe: {Title}", request.Title);

        try
        {
            // Create the recipe
            var recipe = new Core.Domain.Entities.Recipe
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                PhotoUrl = request.PhotoUrl,
                IsFavorite = request.IsFavorite,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = createdByUserId
            };

            // Add ingredients
            var ingredients = request.Ingredients.Select(i => new Core.Domain.Entities.Ingredient
            {
                Id = Guid.NewGuid(),
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
                RecipeId = recipe.Id
            }).ToList();

            // Add steps
            var steps = request.Steps.Select(s => new Core.Domain.Entities.Step
            {
                Id = Guid.NewGuid(),
                StepNumber = s.StepNumber,
                InstructionText = s.InstructionText,
                RecipeId = recipe.Id
            }).ToList();

            // Add to context
            _dbContext.Recipes.Add(recipe);
            _dbContext.Ingredients.AddRange(ingredients);
            _dbContext.Steps.AddRange(steps);

            // Save to database
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Recipe created successfully with ID: {RecipeId}", recipe.Id);

            // Return response
            return new CreateRecipeResponse
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                Category = recipe.Category,
                PhotoUrl = recipe.PhotoUrl,
                IsFavorite = recipe.IsFavorite,
                CreatedAt = recipe.CreatedAt,
                CreatedByUserId = recipe.CreatedByUserId,
                Ingredients = ingredients.Select(i => new IngredientResponse
                {
                    Id = i.Id,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }).ToList(),
                Steps = steps.Select(s => new StepResponse
                {
                    Id = s.Id,
                    StepNumber = s.StepNumber,
                    InstructionText = s.InstructionText
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recipe: {Title}", request.Title);
            throw;
        }
    }

    public async Task<SearchRecipeResponse> SearchRecipesAsync(SearchRecipeRequest request)
    {
        _logger.LogInformation("Searching recipes with text: {SearchText}, Page: {PageNumber}, Size: {PageSize}", 
            request.SearchText, request.PageNumber, request.PageSize);

        try
        {
            // Build the search query
            var query = _dbContext.Recipes
                .Include(r => r.CreatedByUser)
                .Include(r => r.Ingredients)
                .Include(r => r.Steps)
                .Where(r => 
                    r.Title.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.Category.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.Ingredients.Any(i => i.Name.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    r.Steps.Any(s => s.InstructionText.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase))
                );

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            
            // Calculate pagination
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var hasNextPage = request.PageNumber < totalPages;
            var hasPreviousPage = request.PageNumber > 1;

            // Apply pagination and get results
            var recipes = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new RecipeSearchResult
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Category = r.Category,
                    PhotoUrl = r.PhotoUrl,
                    IsFavorite = r.IsFavorite,
                    CreatedAt = r.CreatedAt,
                    CreatedByUserName = r.CreatedByUser != null ? r.CreatedByUser.Username : "Unknown",
                    IngredientCount = r.Ingredients.Count,
                    StepCount = r.Steps.Count
                })
                .ToListAsync();

            _logger.LogInformation("Search completed. Found {TotalCount} recipes, returning {ResultCount} for page {PageNumber}", 
                totalCount, recipes.Count, request.PageNumber);

            return new SearchRecipeResponse
            {
                Recipes = recipes,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching recipes with text: {SearchText}", request.SearchText);
            throw;
        }
    }
}
