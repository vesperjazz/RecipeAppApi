using System;
using System.Collections.Generic;
using Xunit;
using Lambdas.Recipe.DTOs;

namespace RecipeAppApi.Tests;

public class RecipeServiceTests
{
    [Fact]
    public void SearchRecipeRequest_Validation_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var validRequest = new SearchRecipeRequest
        {
            SearchText = "pasta",
            PageSize = 20,
            PageNumber = 1
        };

        // Assert
        Assert.Equal("pasta", validRequest.SearchText);
        Assert.Equal(20, validRequest.PageSize);
        Assert.Equal(1, validRequest.PageNumber);
    }

    [Fact]
    public void SearchRecipeResponse_Properties_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var response = new SearchRecipeResponse();

        // Assert
        Assert.NotNull(response.Recipes);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(0, response.PageNumber);
        Assert.Equal(0, response.PageSize);
        Assert.Equal(0, response.TotalPages);
        Assert.False(response.HasNextPage);
        Assert.False(response.HasPreviousPage);
    }

    [Fact]
    public void RecipeSearchResult_Properties_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var result = new RecipeSearchResult();

        // Assert
        Assert.Equal(Guid.Empty, result.Id);
        Assert.Equal(string.Empty, result.Title);
        Assert.Equal(string.Empty, result.Description);
        Assert.Equal(string.Empty, result.Category);
        Assert.Null(result.PhotoUrl);
        Assert.False(result.IsFavorite);
        Assert.Equal(DateTime.MinValue, result.CreatedAt);
        Assert.Equal(string.Empty, result.CreatedByUserName);
        Assert.Equal(0, result.IngredientCount);
        Assert.Equal(0, result.StepCount);
    }

    // Create Recipe Tests
    [Fact]
    public void CreateRecipeRequest_Validation_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            Description = "A test recipe description",
            Category = "Test Category",
            PhotoUrl = "https://example.com/photo.jpg",
            IsFavorite = true,
            Ingredients = new List<CreateIngredientRequest>
            {
                new CreateIngredientRequest
                {
                    Name = "Test Ingredient",
                    Quantity = "1",
                    Unit = "cup"
                }
            },
            Steps = new List<CreateStepRequest>
            {
                new CreateStepRequest
                {
                    StepNumber = 1,
                    InstructionText = "Test step instruction"
                }
            }
        };

        // Assert
        Assert.Equal("Test Recipe", request.Title);
        Assert.Equal("A test recipe description", request.Description);
        Assert.Equal("Test Category", request.Category);
        Assert.Equal("https://example.com/photo.jpg", request.PhotoUrl);
        Assert.True(request.IsFavorite);
        Assert.Single(request.Ingredients);
        Assert.Single(request.Steps);
    }

    [Fact]
    public void CreateIngredientRequest_Properties_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var ingredient = new CreateIngredientRequest
        {
            Name = "Flour",
            Quantity = "2",
            Unit = "cups"
        };

        // Assert
        Assert.Equal("Flour", ingredient.Name);
        Assert.Equal("2", ingredient.Quantity);
        Assert.Equal("cups", ingredient.Unit);
    }

    [Fact]
    public void CreateStepRequest_Properties_ShouldBeInitializedCorrectly()
    {
        // Arrange & Act
        var step = new CreateStepRequest
        {
            StepNumber = 3,
            InstructionText = "Mix ingredients thoroughly"
        };

        // Assert
        Assert.Equal(3, step.StepNumber);
        Assert.Equal("Mix ingredients thoroughly", step.InstructionText);
    }

    [Fact]
    public void CreateRecipeRequest_EmptyCollections_ShouldBeInitialized()
    {
        // Arrange & Act
        var request = new CreateRecipeRequest();

        // Assert
        Assert.NotNull(request.Ingredients);
        Assert.NotNull(request.Steps);
        Assert.Empty(request.Ingredients);
        Assert.Empty(request.Steps);
    }

    [Fact]
    public void CreateRecipeRequest_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var request = new CreateRecipeRequest();

        // Assert
        Assert.Equal(string.Empty, request.Title);
        Assert.Equal(string.Empty, request.Description);
        Assert.Equal(string.Empty, request.Category);
        Assert.Null(request.PhotoUrl);
        Assert.False(request.IsFavorite);
    }

    [Fact]
    public void CreateRecipeRequest_WithMultipleIngredientsAndSteps_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var request = new CreateRecipeRequest
        {
            Title = "Complex Recipe",
            Description = "A recipe with multiple ingredients and steps",
            Category = "Complex",
            Ingredients = new List<CreateIngredientRequest>
            {
                new CreateIngredientRequest { Name = "Ingredient 1", Quantity = "1", Unit = "unit" },
                new CreateIngredientRequest { Name = "Ingredient 2", Quantity = "2", Unit = "units" },
                new CreateIngredientRequest { Name = "Ingredient 3", Quantity = "3", Unit = "units" }
            },
            Steps = new List<CreateStepRequest>
            {
                new CreateStepRequest { StepNumber = 1, InstructionText = "Step 1" },
                new CreateStepRequest { StepNumber = 2, InstructionText = "Step 2" },
                new CreateStepRequest { StepNumber = 3, InstructionText = "Step 3" }
            }
        };

        // Assert
        Assert.Equal(3, request.Ingredients.Count);
        Assert.Equal(3, request.Steps.Count);
        Assert.Equal("Ingredient 2", request.Ingredients[1].Name);
        Assert.Equal("Step 2", request.Steps[1].InstructionText);
    }
}
