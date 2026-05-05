namespace PandaCubeTimer_API.Models.Responses;

public class CompleteTimerSyncResponse
{
    public DateTime ServerTimeUtc { get; set; }
    
    // Stuff updated on the server from other devices
    public List<SessionDTO> ServerSessions { get; set; } = new();
    public List<SolveDTO> ServerSolves { get; set; } = new();
    
    // IDs of the items the server successfully saved from our request
    public List<Guid> AcknowledgedSessionIds { get; set; } = new();
    public List<Guid> AcknowledgedSolveIds { get; set; } = new();
}