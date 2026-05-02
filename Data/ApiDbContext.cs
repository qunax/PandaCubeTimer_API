using Microsoft.EntityFrameworkCore;
using PandaCubeTimer_API.Models;

namespace PandaCubeTimer_API.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) 
    { 
    }

    
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Solve> Solves => Set<Solve>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Discipline> Disciplines => Set<Discipline>();
}