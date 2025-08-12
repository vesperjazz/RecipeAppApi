using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Domain.Entities;

namespace Infrastructure.Persistence.Configurations;

public class StepConfiguration : IEntityTypeConfiguration<Step>
{
    public void Configure(EntityTypeBuilder<Step> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.StepNumber)
            .IsRequired();
            
        builder.Property(s => s.InstructionText)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(s => s.RecipeId)
            .IsRequired();
            
        // Relationships
        builder.HasOne(s => s.Recipe)
            .WithMany(r => r.Steps)
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Ensure step numbers are unique within a recipe
        builder.HasIndex(s => new { s.RecipeId, s.StepNumber })
            .IsUnique();
    }
}
