using Microsoft.EntityFrameworkCore;

namespace Lambdas.Authorizer;

public class AuthorizerDbContext : DbContext
{
    public AuthorizerDbContext(DbContextOptions<AuthorizerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // TODO: Add entity configurations here
        // Potential entities:
        // - Users (Id, Username, Email, Role, IsActive, CreatedAt, UpdatedAt)
        // - RevokedTokens (Id, TokenHash, RevokedAt, Reason, RevokedBy)
        // - AuthorizationAuditLogs (Id, UserId, RequestId, Token, IsValid, Timestamp, IPAddress, UserAgent)
        // - UserPermissions (Id, UserId, Resource, Action, GrantedAt, GrantedBy)
    }
}
