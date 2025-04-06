using Microsoft.EntityFrameworkCore;

namespace AuthenticationWebApi;

public class AuthenticationContext(DbContextOptions options) : DbContext(options);