namespace Core.Domain.Entities;

public class Ingredient
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public Guid RecipeId { get; set; }
    
    // Navigation properties
    public virtual Recipe Recipe { get; set; } = null!;
}
