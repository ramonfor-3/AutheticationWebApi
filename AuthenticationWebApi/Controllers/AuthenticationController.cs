using System.Security.Claims;
using AuthenticationWebApi.Entities;
using AuthenticationWebApi.Filters;
using AuthenticationWebApi.ServiceInterface;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController(
    ILogger<AuthenticationController> logger,
    IAuthService authService, 
    ILoginService loginService,
    IPermissionService permissionService)
    : ControllerBase
{
    [HttpPost("RegisterUser")]
    public async Task<User> RegisterUser([FromBody] RegisterRequest user) => await authService.RegisterAsync(user);
    

    [HttpPost("Login")]
    public async Task<AuthResponse> LoginUser([FromBody] LoginRequest user) => await loginService.LoginAsync(user);
    
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest request)
    {
        var userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id : throw new UnauthorizedAccessException("No se pudo determinar el usuario.");
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
    [HttpGet("GetUserPermissions")]
    public async Task<List<string>> GetUserPermissions([FromQuery ]HasPermissionFilter filter)
    {
        var permissions = await permissionService.GetUserPermissionsAsync(filter);
        return permissions;
    }
}