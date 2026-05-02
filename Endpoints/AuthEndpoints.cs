using PandaCubeTimer_API.Data;
using PandaCubeTimer_API.Helpers;

namespace PandaCubeTimer_API.Endpoints;

using Models;
using Microsoft.EntityFrameworkCore;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
    }

    
    
    private static async Task<IResult> Register(UserRegisterDTO request, ApiDbContext db)
    {
        if (await db.Users.AnyAsync(u => u.Username == request.Username))
            return Results.BadRequest(new { Message = "Username taken" });

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Results.Ok(new { Message = "Success" });
    }

    private static async Task<IResult> Login(UserLoginDTO request, ApiDbContext db, IConfiguration config)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Results.Unauthorized();
        }
        
        var accessToken = JwtTokenHelper.GenerateJwtToken(user, config);
        
        var refreshTokenString = JwtTokenHelper.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            DeviceName = request.DeviceName ?? "Unknown Device"
        };
        
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();
        
        return Results.Ok(new LoginResponseDTO(
            user.Id,
            user.Username,
            accessToken,
            refreshTokenString
        ));
    }
}