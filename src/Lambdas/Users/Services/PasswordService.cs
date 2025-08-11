using System;
using System.Security.Cryptography;
using System.Text;

namespace Lambdas.Users.Services;

public class PasswordService : IPasswordService
{
    public (string hash, string salt) HashPassword(string password)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));

        // Generate a random salt
        var saltBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        
        var salt = Convert.ToBase64String(saltBytes);
        
        // Combine password and salt, then hash
        var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(combinedBytes);
            var hash = Convert.ToBase64String(hashBytes);
            return (hash, salt);
        }
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));
        if (hash == null)
            throw new ArgumentNullException(nameof(hash));
        if (salt == null)
            throw new ArgumentNullException(nameof(salt));

        // Use the provided salt to hash the password for verification
        var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(combinedBytes);
            var computedHash = Convert.ToBase64String(hashBytes);
            return computedHash == hash;
        }
    }
}
