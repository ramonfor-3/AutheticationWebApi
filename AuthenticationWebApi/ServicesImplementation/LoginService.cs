using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthenticationWebApi.Entities;
using AuthenticationWebApi.ServiceInterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationWebApi.ServicesImplementation;

public class LoginService(AuthenticationContext dbContext, IConfiguration _configuration) : ILoginService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest model)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Username == model.Username);
        
        if(user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            throw new Exception("Invalid username or password");
        
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        await dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await dbContext.SaveChangesAsync();
        
        return new AuthResponse { 
            AccessToken = accessToken, 
            RefreshToken = refreshToken, 
            Expiration = DateTime.UtcNow.AddMinutes(30)
            
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
        if (token != null)
        {
            token.IsRevoked = true;
            dbContext.RefreshTokens.Update(token);
            await dbContext.SaveChangesAsync();
        }
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "User") // Puedes agregar roles aquí si es necesario
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedRefreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken && r.IsRevoked == false);

        if (storedRefreshToken == null || storedRefreshToken.ExpiryDate <= DateTime.UtcNow)
            throw new Exception("Refresh token no válido o expirado.");
        
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == storedRefreshToken.UserId);
        
        var newAccessToken = GenerateAccessToken(user);
        
        storedRefreshToken.IsRevoked = true;
        dbContext.RefreshTokens.Update(storedRefreshToken);
        await dbContext.SaveChangesAsync();
        
        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(30)
        };
    }
    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomBytes);
        }
    
        return Convert.ToBase64String(randomBytes);
    }

}