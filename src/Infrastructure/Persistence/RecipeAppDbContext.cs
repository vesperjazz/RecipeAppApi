using Microsoft.EntityFrameworkCore;
using Core.Domain.Entities;

namespace Infrastructure.Persistence;

public class RecipeAppDbContext : DbContext
{
    public RecipeAppDbContext(DbContextOptions<RecipeAppDbContext> options) : base(options)
    {
    }

    // User and Role management
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipeAppDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
