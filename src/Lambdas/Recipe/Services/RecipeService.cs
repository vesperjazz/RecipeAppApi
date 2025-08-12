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
}
