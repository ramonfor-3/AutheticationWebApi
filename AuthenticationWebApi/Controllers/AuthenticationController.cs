using System.Security.Claims;
using AuthenticationWebApi.Entities;
using AuthenticationWebApi.ServiceInterface;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController(
    ILogger<AuthenticationController> logger,
    IAuthService authService, 
    ILoginService loginService)
    : ControllerBase
{
    [HttpPost("RegisterUser")]
    public async Task<User> RegisterUser([FromBody] RegisterRequest user) => await authService.RegisterAsync(user);
    

    [HttpPost("Login")]
    public async Task<AuthResponse> LoginUser([FromBody] LoginRequest user) => await loginService.LoginAsync(user);
    
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await authService.ChangePasswordAsync(userId, request);
        return Ok("Contraseña actualizada con éxito.");
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await loginService.LogoutAsync(request.TokenId);
        return Ok(new { message = "Sesión cerrada correctamente." });
    }
    
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var authResponse = await loginService.RefreshTokenAsync(request.TokenId, request.RefreshToken);
        return Ok(authResponse);
    }
    
}