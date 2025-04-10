using AuthenticationWebApi.Entities;
using AuthenticationWebApi.ServiceInterface;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController(
    ILogger<AuthenticationController> logger,
    IAuthService authService, ILoginService loginService)
    : ControllerBase
{
    [HttpPost("RegisterUser")]
    public async Task<User> RegisterUser([FromBody] RegisterRequest user)
    {
        var newUser = await authService.RegisterAsync(user);
        return newUser;
    }

    [HttpPost("LoginUser")]
    public async Task<AuthResponse> LoginUser([FromBody] LoginRequest user)
    {
        var login = await loginService.LoginAsync(user);
        return login;
    }
}