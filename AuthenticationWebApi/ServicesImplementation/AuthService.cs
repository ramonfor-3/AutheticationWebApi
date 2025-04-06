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
            IsActive = true
        };
        
        await context.Users.AddAsync(newUser);
        await context.SaveChangesAsync();
        return newUser;

    }
}