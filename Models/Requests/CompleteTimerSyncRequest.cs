namespace PandaCubeTimer_API.Models.Requests;

public class CompleteTimerSyncRequest
{
    public DateTimeOffset LastSyncTimeUtc { get; set; }
    public List<SessionDTO> UnsyncedSessions { get; set; } = new();
    public List<SolveDTO> UnsyncedSolves { get; set; } = new();
}