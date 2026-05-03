using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PandaCubeTimer_API.Data;
using PandaCubeTimer_API.Models;

namespace PandaCubeTimer_API.Endpoints;

public static class SyncEndpoints
{
    public static void MapSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sync").RequireAuthorization();

        group.MapPost("/sessions/syncFull", SyncAllSessionsAsync)
            .Produces<List<SessionSyncDTO>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        
        group.MapPost("/sessions/{sessionId:guid}/solves", SyncSessionSolvesAsync)
            .Produces<SolveSyncResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> SyncAllSessionsAsync(List<SessionSyncDTO> localSessions, 
        ApiDbContext db, ClaimsPrincipal user)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // 1. Загружаем все сессии этого юзера из базы сервера
        var serverSessions = await db.Sessions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        // 2. Ищем сессии, которые пришли с телефона, но которых еще нет на сервере
        var serverSessionIds = serverSessions.Select(s => s.Id).ToHashSet();
        
        var newSessionsToInsert = localSessions
            .Where(ls => !serverSessionIds.Contains(ls.Id))
            .Select(ls => new Session
            {
                Id = ls.Id,
                UserId = userId,
                Name = ls.Name,
                DisciplineId = ls.DisciplineId,
                IsDeleted = ls.IsDeleted,
                UpdatedAt = DateTime.UtcNow
            })
            .ToList();

        // Если нашли новые — сохраняем их в базу
        if (newSessionsToInsert.Any())
        {
            db.Sessions.AddRange(newSessionsToInsert);
            await db.SaveChangesAsync();
            
            // Добавляем их в наш локальный список, чтобы сразу вернуть пользователю
            serverSessions.AddRange(newSessionsToInsert);
        }

        // 3. Формируем финальный список ВСЕХ сессий (и старых серверных, и только что добавленных локальных)
        var finalSyncList = serverSessions.Select(s => new SessionSyncDTO(
            s.Id,
            s.Name,
            s.DisciplineId,
            s.IsDeleted
        )).ToList();

        // 4. Отдаем телефону
        return Results.Ok(finalSyncList);
    }

    public static async Task<IResult> SyncSessionSolvesAsync(
        Guid sessionId, 
        SolveSyncRequest request, 
        ApiDbContext db, 
        ClaimsPrincipal user)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // 1. Security Check: Does this session belong to this user?
        var ownsSession = await db.Sessions.AnyAsync(s => s.Id == sessionId && s.UserId == userId);
        if (!ownsSession)
        {
            return Results.Forbid(); // 403 Forbidden
        }

        // 2. Process changes coming from the mobile app (Upsert logic)
        if (request.ClientChanges.Any())
        {
            var incomingIds = request.ClientChanges.Select(c => c.Id).ToList();
            
            // Fetch existing solves from DB to compare
            var existingSolves = await db.Solves
                .Where(s => incomingIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id);

            foreach (var incoming in request.ClientChanges)
            {
                if (existingSolves.TryGetValue(incoming.Id, out var existingSolve))
                {
                    // Conflict resolution: update ONLY if phone's version is newer
                    if (incoming.UpdatedAt > existingSolve.UpdatedAt)
                    {
                        existingSolve.SolveTimeSeconds = incoming.SolveTimeSeconds;
                        existingSolve.IsPlusTwo = incoming.IsPlusTwo;
                        existingSolve.IsDNF = incoming.IsDNF;
                        existingSolve.Scramble = incoming.Scramble;
                        existingSolve.DateTime = incoming.DateTime;
                        existingSolve.Comment = incoming.Comment;
                        existingSolve.IsDeleted = incoming.IsDeleted;
                        existingSolve.UpdatedAt = incoming.UpdatedAt;
                    }
                }
                else
                {
                    // It's a completely new solve made offline
                    db.Solves.Add(new Solve
                    {
                        Id = incoming.Id,
                        SessionId = sessionId, // Force assign to the URL session
                        SolveTimeSeconds = incoming.SolveTimeSeconds,
                        IsPlusTwo = incoming.IsPlusTwo,
                        IsDNF = incoming.IsDNF,
                        Scramble = incoming.Scramble,
                        DateTime = incoming.DateTime,
                        Comment = incoming.Comment,
                        IsDeleted = incoming.IsDeleted,
                        UpdatedAt = incoming.UpdatedAt
                    });
                }
            }
            
            await db.SaveChangesAsync();
        }

        // 3. Find changes on the server that the phone doesn't know about yet
        // (e.g. solves uploaded from your iPad yesterday)
        var serverChanges = await db.Solves
            .Where(s => s.SessionId == sessionId && s.UpdatedAt > request.LastSyncTimeUtc)
            .Select(s => new SolveDTO(
                s.Id,
                s.SolveTimeSeconds,
                s.IsPlusTwo,
                s.IsDNF,
                s.Scramble,
                s.DateTime,
                s.Comment,
                s.IsDeleted,
                s.UpdatedAt
            ))
            .ToListAsync();

        // 4. Return new sync time and server changes
        return Results.Ok(new SolveSyncResponse(
            ServerTimeUtc: DateTime.UtcNow,
            ServerChanges: serverChanges
        ));
    }
}