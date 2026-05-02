namespace PandaCubeTimer_API.Models.DTOs;

public class SessionMetadataDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisciplineName { get; set; } = string.Empty;
}