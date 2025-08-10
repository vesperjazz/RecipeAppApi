using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class RecipeAppDbContext : DbContext
{
    public RecipeAppDbContext(DbContextOptions<RecipeAppDbContext> options) : base(options)
    {
    }

    // DbSets will be added here as entities are created
    // Example:
    // public DbSet<Recipe> Recipes { get; set; }
    // public DbSet<User> Users { get; set; }
    // public DbSet<AuthToken> AuthTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipeAppDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
