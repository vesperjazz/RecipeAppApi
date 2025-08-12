using System;
using System.Collections.Generic;

namespace Lambdas.Recipe.DTOs;

public class CreateRecipeResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public List<IngredientResponse> Ingredients { get; set; } = new();
    public List<StepResponse> Steps { get; set; } = new();
}

public class IngredientResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}

public class StepResponse
{
    public Guid Id { get; set; }
    public int StepNumber { get; set; }
    public string InstructionText { get; set; } = string.Empty;
}
