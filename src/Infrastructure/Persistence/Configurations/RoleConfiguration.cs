using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Id)
            .HasColumnType("CHAR(36)")
            .IsRequired();
            
        builder.Property(r => r.RoleName)
            .HasColumnType("VARCHAR(50)")
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(r => r.Description)
            .HasColumnType("VARCHAR(255)")
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(r => r.CreatedDate)
            .HasColumnType("DATETIME(6)")
            .IsRequired();
            
        builder.Property(r => r.UpdatedDate)
            .HasColumnType("DATETIME(6)")
            .IsRequired();
            
        // Indexes
        builder.HasIndex(r => r.RoleName)
            .IsUnique();
            
                       // Relationships
               builder.HasMany(r => r.UserRoles)
                   .WithOne(ur => ur.Role)
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
               // Seed initial data
               builder.HasData(
                   new Role
                   {
                       Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                       RoleName = "User",
                       Description = "Standard user with basic access to the application",
                       CreatedDate = DateTime.UtcNow,
                       UpdatedDate = DateTime.UtcNow
                   },
                   new Role
                   {
                       Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                       RoleName = "Admin",
                       Description = "Administrator with elevated privileges and user management capabilities",
                       CreatedDate = DateTime.UtcNow,
                       UpdatedDate = DateTime.UtcNow
                   },
                   new Role
                   {
                       Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                       RoleName = "SuperAdmin",
                       Description = "Super administrator with full system access and configuration capabilities",
                       CreatedDate = DateTime.UtcNow,
                       UpdatedDate = DateTime.UtcNow
                   }
               );
    }
}
