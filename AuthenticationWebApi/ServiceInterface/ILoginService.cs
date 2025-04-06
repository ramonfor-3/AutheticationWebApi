using AuthenticationWebApi.Entities;

namespace AuthenticationWebApi.ServiceInterface;

public interface ILoginService
{
    Task<AuthResponse> LoginAsync(LoginRequest model);
    Task LogoutAsync(string refreshToken);

    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}