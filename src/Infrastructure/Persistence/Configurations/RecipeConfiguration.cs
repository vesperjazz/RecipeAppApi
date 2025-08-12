using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Domain.Entities;

namespace Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(r => r.Category)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(r => r.PhotoUrl)
            .HasMaxLength(500);
            
        builder.Property(r => r.CreatedAt)
            .IsRequired();
            
        builder.Property(r => r.CreatedByUserId)
            .IsRequired();
            
        // Relationships
        builder.HasOne(r => r.CreatedByUser)
            .WithMany(u => u.Recipes)
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(r => r.Ingredients)
            .WithOne(i => i.Recipe)
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(r => r.Steps)
            .WithOne(s => s.Recipe)
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
