namespace PandaCubeTimer_API.Models;

// То, что мы получаем от MAUI при регистрации
public record UserRegisterDTO(string Username, string Password);

// То, что мы получаем от MAUI при логине
public record UserLoginDTO(string Username, string Password, string? DeviceName);

// То, что мы отправляем в MAUI после успешного входа
public record LoginResponseDTO(
    Guid UserId, 
    string Username, 
    string AccessToken, 
    string RefreshToken
);

public record SessionSyncDTO(
    Guid Id,
    string Name,
    string DisciplineId,
    bool IsDeleted
);

public record SolveSyncRequest(
    DateTime LastSyncTimeUtc, // When the phone successfully synced this session last time
    List<SolveDTO> ClientChanges // Solves created, modified, or deleted while offline
);

// Unified Solve object for network transfer
public record SolveDTO(
    Guid Id,
    double SolveTimeSeconds,
    bool IsPlusTwo,
    bool IsDNF,
    string Scramble,
    DateTime DateTime,
    string? Comment,
    bool IsDeleted,
    DateTime UpdatedAt
);

// Response from Server
public record SolveSyncResponse(
    DateTime ServerTimeUtc, // New time the phone must save as "LastSyncTimeUtc"
    List<SolveDTO> ServerChanges // Solves changed on server (e.g., from another device)
);