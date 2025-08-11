using System.Collections.Generic;
using System.Security.Claims;
using Core.Domain.Entities;

namespace Lambdas.Users.Services;

public interface IJwtService
{
    string GenerateAccessTokenAsync(User user, IEnumerable<string> roles);
    IEnumerable<Claim> GenerateClaimsAsync(User user, IEnumerable<string> roles);
    bool ValidatePasswordAsync(string password, string passwordHash, string passwordSalt);
}

