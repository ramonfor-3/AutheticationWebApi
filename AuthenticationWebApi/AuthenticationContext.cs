using AuthenticationWebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationWebApi;

public class AuthenticationContext: DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public AuthenticationContext(DbContextOptions options) : base(options)
    {
        
    }
}