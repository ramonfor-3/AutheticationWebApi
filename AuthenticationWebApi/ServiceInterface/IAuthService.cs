using AuthenticationWebApi.Entities;

namespace AuthenticationWebApi.ServiceInterface;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterRequest model);
}