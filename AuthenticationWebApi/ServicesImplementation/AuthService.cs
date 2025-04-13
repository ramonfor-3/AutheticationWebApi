using AuthenticationWebApi.Entities;
using AuthenticationWebApi.ServiceInterface;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationWebApi.ServicesImplementation;

public class AuthService(AuthenticationContext context, IConfiguration configuration)
    : IAuthService
{
    public async Task<User> RegisterAsync(RegisterRequest model)
    {
        if (await context.Users.AnyAsync(x => x.Username == model.Username && x.Email == model.Email)) 
            throw new Exception("Username and email already exists");

        var newUser = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            CreatedAt = DateTime.UtcNow,
            PasswordLastChangedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        await context.Users.AddAsync(newUser);
        await context.SaveChangesAsync();
        return newUser;

    }

    public async Task ChangePasswordAsync(int userId, PasswordChangeRequest request)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Usuario no encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new Exception("Contraseña actual incorrecta.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordLastChangedAt = DateTime.UtcNow;
        
        var activeTokens = await context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }

        await context.SaveChangesAsync();
    }
}