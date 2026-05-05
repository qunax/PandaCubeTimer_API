using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PandaCubeTimer_API.Data;
using PandaCubeTimer_API.Models;
using PandaCubeTimer_API.Models.Requests;
using PandaCubeTimer_API.Models.Responses;

namespace PandaCubeTimer_API.Endpoints;

public static class SyncEndpoints
{
    public static void MapSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sync").RequireAuthorization();
        
        group.MapPost("/completeTimerSync", CompleteTimerSyncAsync)
            .Produces<CompleteTimerSyncResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }


    private static async Task<IResult> CompleteTimerSyncAsync(CompleteTimerSyncRequest request, ApiDbContext db, ClaimsPrincipal user)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = new CompleteTimerSyncResponse() { ServerTimeUtc = DateTime.UtcNow };

        // ==========================================
        // 1. PROCESS INCOMING SESSIONS (From Mobile)
        // ==========================================
        if (request.UnsyncedSessions.Any())
        {
            var incomingIds = request.UnsyncedSessions.Select(s => s.Id).ToList();
            var existingSessions = await db.Sessions.Where(s => incomingIds.Contains(s.Id) && s.UserId == userId).ToDictionaryAsync(s => s.Id);

            foreach (var incoming in request.UnsyncedSessions)
            {
                if (existingSessions.TryGetValue(incoming.Id, out var existing))
                {
                    // Conflict resolution: Only update if the phone's edit is newer
                    if (incoming.UpdatedAt > existing.UpdatedAt)
                    {
                        existing.Name = incoming.Name;
                        existing.IsDeleted = incoming.IsDeleted;
                        existing.DisciplineId = incoming.DisciplineId;
                        existing.UpdatedAt = incoming.UpdatedAt.UtcDateTime;
                        response.AcknowledgedSessionIds.Add(incoming.Id);
                    }
                }
                else
                {
                    // Insert new
                    db.Sessions.Add(new Session
                    {
                        Id = incoming.Id, 
                        UserId = userId, 
                        Name = incoming.Name, 
                        DisciplineId = incoming.DisciplineId,
                        IsDeleted = incoming.IsDeleted, 
                        CreatedAt = incoming.CreatedAt.UtcDateTime,
                        UpdatedAt = incoming.UpdatedAt.UtcDateTime,
                    });
                    response.AcknowledgedSessionIds.Add(incoming.Id);
                }
            }
            await db.SaveChangesAsync();
        }

        // ==========================================
        // 2. PROCESS INCOMING SOLVES (From Mobile)
        // ==========================================
        if (request.UnsyncedSolves.Any())
        {
            var incomingIds = request.UnsyncedSolves.Select(s => s.Id).ToList();
            // Security: Ensure we only touch solves linked to the user's sessions
            var userSessionIds = await db.Sessions.Where(s => s.UserId == userId).Select(s => s.Id).ToListAsync();
            var existingSolves = await db.Solves.Where(s => incomingIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id);

            foreach (var incoming in request.UnsyncedSolves)
            {
                if (!userSessionIds.Contains(incoming.SessionId)) continue; // Skip if session doesn't belong to user

                if (existingSolves.TryGetValue(incoming.Id, out var existing))
                {
                    if (incoming.UpdatedAt > existing.UpdatedAt)
                    {
                        existing.SolveTimeSeconds = incoming.SolveTimeSeconds;
                        existing.IsDeleted = incoming.IsDeleted;
                        existing.UpdatedAt = incoming.UpdatedAt.UtcDateTime;
                        existing.Comment = incoming.Comment;
                        existing.IsDNF = incoming.IsDNF;
                        existing.IsPlusTwo = incoming.IsPlusTwo;
                        response.AcknowledgedSolveIds.Add(incoming.Id);
                    }
                }
                else
                {
                    db.Solves.Add(new Solve
                    {
                        Id = incoming.Id, 
                        SessionId = incoming.SessionId, 
                        SolveTimeSeconds = incoming.SolveTimeSeconds, 
                        IsPlusTwo = incoming.IsPlusTwo, 
                        IsDNF = incoming.IsDNF, 
                        Scramble = incoming.Scramble, 
                        CreatedAt = incoming.CreatedAt.UtcDateTime, 
                        Comment = incoming.Comment, 
                        IsDeleted = incoming.IsDeleted, 
                        UpdatedAt = incoming.UpdatedAt.UtcDateTime,
                    });
                    response.AcknowledgedSolveIds.Add(incoming.Id);
                }
            }
            await db.SaveChangesAsync();
        }

        // ==========================================
        // 3. FETCH NEW SERVER DATA FOR MOBILE
        // ==========================================
        var userSessions = await db.Sessions.Where(s => s.UserId == userId).ToListAsync();
        var validSessionIds = userSessions.Select(s => s.Id).ToList();

        // Grab everything changed on the server AFTER the mobile's last sync
        response.ServerSessions = userSessions
            .Where(s => s.UpdatedAt > request.LastSyncTimeUtc.UtcDateTime)
            .Select(s => new SessionDTO
            {
                Id = s.Id, 
                Name = s.Name, 
                DisciplineId = s.DisciplineId,
                IsDeleted = s.IsDeleted, 
                UpdatedAt = s.UpdatedAt,
                CreatedAt = s.CreatedAt
            })
            .ToList();

        response.ServerSolves = await db.Solves
            .Where(s => validSessionIds.Contains(s.SessionId) && s.UpdatedAt > request.LastSyncTimeUtc.UtcDateTime)
            .Select(s => new SolveDTO
            {
                Id = s.Id,
                SessionId = s.SessionId, 
                SolveTimeSeconds = s.SolveTimeSeconds,
                IsPlusTwo = s.IsPlusTwo, 
                IsDNF = s.IsDNF, 
                Scramble = s.Scramble,
                CreatedAt = s.CreatedAt, 
                Comment = s.Comment, 
                IsDeleted = s.IsDeleted, 
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();

        return Results.Ok(response);
    }
}