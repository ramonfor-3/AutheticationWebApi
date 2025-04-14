using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthenticationWebApi.Entities;
using AuthenticationWebApi.Exceptions;
using AuthenticationWebApi.ServiceInterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationWebApi.ServicesImplementation;

public class LoginService(
    AuthenticationContext dbContext, 
    IConfiguration configuration, 
    IHttpContextAccessor httpContextAccessor, ILogger<LoginService> logger) : ILoginService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest model)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Username == model.Username);
        if (user == null)
            throw new Exception("Usuario no encontrado");
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var timeLeft = user.LockoutEnd.Value - DateTime.UtcNow;
            throw new Exception($"Tu cuenta está bloqueada. Intenta nuevamente en {timeLeft.Minutes} minutos.");
        }

        if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                await dbContext.SaveChangesAsync();
                throw new Exception("Has superado el número de intentos. Tu cuenta ha sido bloqueada por 15 minutos.");
            }

            await dbContext.SaveChangesAsync();
            throw new Exception("Usuario o contraseña incorrectos.");
        }
        
        var passwordExpiryDate = user.PasswordLastChangedAt.AddDays(90);

        if (DateTime.UtcNow > passwordExpiryDate)
        {
            logger.LogWarning("La contraseña del usuario {Username} ha expirado.", user.Username);
            throw new PasswordExpiredException("Tu contraseña ha expirado. Por favor, cámbiala para continuar.");
        }

        logger.LogInformation("Inicio de sesión exitoso para el usuario: {Username}", user.Username);

        var accessToken = await GenerateAccessToken(user);
        var tokenId = Guid.NewGuid().ToString();
        var refreshToken = GenerateRefreshToken();
        var hashedToken = BCrypt.Net.BCrypt.HashPassword(refreshToken);

        var userAgent = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        var oldTokens = await dbContext.RefreshTokens
            .Where(r => r.UserId == user.Id && r.UserAgent == userAgent && r.IsRevoked == false)
            .ToListAsync();

        foreach (var t in oldTokens)
        {
            t.IsRevoked = true;
            //dbContext.RefreshTokens.Update(t);
        }

        var tokenIssuedAt = DateTime.UtcNow;
        if (user.PasswordLastChangedAt > tokenIssuedAt)
             throw new TokenInvalidException("La contraseña ha sido cambiada, por lo que el token es inválido.");
        

        var refreshTokenEntity = new RefreshToken
        {
            TokenId = tokenId,
            TokenHash = hashedToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        await dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenId = tokenId,
            Expiration = DateTime.UtcNow.AddMinutes(30)
        };
    }

    public async Task LogoutAsync(string tokenId)
    {
        try
        {
            var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenId == tokenId);
            if (token == null)
                throw new TokenInvalidException("Token no válido o no encontrado.");

            token.IsRevoked = true;
            dbContext.RefreshTokens.Update(token);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al intentar cerrar sesión: {ex.Message}");
        }
    }

    private async Task<string> GenerateAccessToken(User user)
    {
        var userContexts = await dbContext.UserCompanyLocations
            .Where(x => x.UserId == user.Id)
            .Include(x => x.Role)
            .Include(x => x.Company)
            .Include(x => x.Location)
            .ToListAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var roleIds = userContexts.Select(x => x.RoleId).Distinct().ToList();

        var rolePermissions = await dbContext.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .ToListAsync();

        var contexts = new List<UserContextDto>();

        foreach (var context in userContexts)
        {
            var permissions = rolePermissions
                .Where(rp => rp.RoleId == context.RoleId)
                .Select(rp => rp.Permission.Code)
                .ToList();

            contexts.Add(new UserContextDto(
                context.CompanyId,
                context.LocationId,
                context.Role.Name,
                permissions
            ));
        }

        var contextJson = System.Text.Json.JsonSerializer.Serialize(contexts);
        claims.Add(new Claim("context", contextJson));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenExpiryMinutes = int.TryParse(configuration["JwtSettings:ExpiryMinutes"], out var minutes)
            ? minutes
            : 30;

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(tokenExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    
    public async Task<AuthResponse> RefreshTokenAsync(string tokenId, string refreshToken)
    {
        var storedRefreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenId == tokenId && !r.IsRevoked);

        if (storedRefreshToken == null || storedRefreshToken.ExpiryDate <= DateTime.UtcNow)
            throw new Exception("Token no válido o expirado.");

        if (!BCrypt.Net.BCrypt.Verify(refreshToken, storedRefreshToken.TokenHash))
            throw new Exception("Token inválido.");

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == storedRefreshToken.UserId);
        var newAccessToken = await GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();
        var newHashedToken = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
        var newTokenId = Guid.NewGuid().ToString();

        storedRefreshToken.IsRevoked = true;
        dbContext.RefreshTokens.Update(storedRefreshToken);

        var newToken = new RefreshToken
        {
            TokenId = newTokenId,
            TokenHash = newHashedToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            UserAgent = storedRefreshToken.UserAgent,
            IpAddress = storedRefreshToken.IpAddress,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.RefreshTokens.AddAsync(newToken);
        await dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            TokenId = newTokenId,
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