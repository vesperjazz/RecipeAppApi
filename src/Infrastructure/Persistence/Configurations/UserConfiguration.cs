using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Id)
            .HasColumnType("CHAR(36)")
            .IsRequired();
            
        builder.Property(u => u.Username)
            .HasColumnType("VARCHAR(50)")
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.Email)
            .HasColumnType("VARCHAR(255)")
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(u => u.PasswordHash)
            .HasColumnType("VARCHAR(255)")
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(u => u.PasswordSalt)
            .HasColumnType("VARCHAR(255)")
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(u => u.CreatedDate)
            .HasColumnType("DATETIME(6)")
            .IsRequired();
            
        builder.Property(u => u.UpdatedDate)
            .HasColumnType("DATETIME(6)")
            .IsRequired();
            
        // Indexes
        builder.HasIndex(u => u.Username)
            .IsUnique();
            
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        // Relationships
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
