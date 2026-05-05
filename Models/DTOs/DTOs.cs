namespace PandaCubeTimer_API.Models;

public record UserRegisterDTO(string Username, string Password);

public record UserLoginDTO(string Username, string Password, string? DeviceName);

public record LoginResponseDTO(
    Guid UserId, 
    string Username, 
    string AccessToken, 
    string RefreshToken
);

public class SessionDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string DisciplineId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class SolveDTO
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; } 
    public double SolveTimeSeconds { get; set; }
    public bool IsPlusTwo { get; set; }
    public bool IsDNF { get; set; }
    public string Scramble { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Comment { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
