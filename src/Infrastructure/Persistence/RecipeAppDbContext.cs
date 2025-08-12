using Microsoft.EntityFrameworkCore;
using Core.Domain.Entities;

namespace Infrastructure.Persistence;

public interface IRecipeAppDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<Role> Roles { get; set; }
    DbSet<UserRole> UserRoles { get; set; }
    DbSet<Recipe> Recipes { get; set; }
    DbSet<Ingredient> Ingredients { get; set; }
    DbSet<Step> Steps { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}

public class RecipeAppDbContext : DbContext, IRecipeAppDbContext
{
    public RecipeAppDbContext(DbContextOptions<RecipeAppDbContext> options) : base(options)
    {
    }

    // User and Role management
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    
    // Recipe management
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Step> Steps { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipeAppDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
