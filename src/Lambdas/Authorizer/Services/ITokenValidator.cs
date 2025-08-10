namespace Lambdas.Authorizer.Services;

public interface ITokenValidator
{
    Task<bool> ValidateTokenAsync(string token);
    Task<string> GetPrincipalIdAsync(string token);
}
