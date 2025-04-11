using AuthenticationWebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationWebApi;

public class AuthenticationContext:DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Permissions> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserCompanyLocation> UserCompanyLocations { get; set; }
    public AuthenticationContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });
        modelBuilder.Entity<User>()
            .HasKey(x => x.Id);
        modelBuilder.Entity<Role>()
            .HasKey(x => x.Id);
        base.OnModelCreating(modelBuilder);
    }
}