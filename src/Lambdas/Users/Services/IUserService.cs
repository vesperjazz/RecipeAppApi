using System.Collections.Generic;
using System.Threading.Tasks;
using Lambdas.Users.DTOs;

namespace Lambdas.Users.Services;

public interface IUserService
{
    Task<string> GetWelcomeMessageAsync();
    Task<IEnumerable<string>> GetUserNamesAsync();
    Task<SignUpResponse> SignUpAsync(SignUpRequest request);
}
