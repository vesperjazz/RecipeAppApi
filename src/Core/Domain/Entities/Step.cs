namespace Core.Domain.Entities;

public class Step
{
    public Guid Id { get; set; }
    public int StepNumber { get; set; }
    public string InstructionText { get; set; } = string.Empty;
    public Guid RecipeId { get; set; }
    
    // Navigation properties
    public virtual Recipe Recipe { get; set; } = null!;
}
