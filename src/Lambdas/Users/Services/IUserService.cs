using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lambdas.Users.Services;

public interface IUserService
{
    Task<string> GetWelcomeMessageAsync();
    Task<IEnumerable<string>> GetUserNamesAsync();
}
