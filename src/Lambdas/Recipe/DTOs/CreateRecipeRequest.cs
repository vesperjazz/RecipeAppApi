using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lambdas.Recipe.DTOs;

public class CreateRecipeRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? PhotoUrl { get; set; }
    
    public bool IsFavorite { get; set; }
    
    [Required]
    public List<CreateIngredientRequest> Ingredients { get; set; } = new();
    
    [Required]
    public List<CreateStepRequest> Steps { get; set; } = new();
}

public class CreateIngredientRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Quantity { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;
}

public class CreateStepRequest
{
    [Required]
    public int StepNumber { get; set; }
    
    [Required]
    [StringLength(500)]
    public string InstructionText { get; set; } = string.Empty;
}
