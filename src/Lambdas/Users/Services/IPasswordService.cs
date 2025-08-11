namespace Lambdas.Users.Services;

public interface IPasswordService
{
    (string hash, string salt) HashPassword(string password);
    bool VerifyPassword(string password, string hash, string salt);
}
