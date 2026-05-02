namespace PandaCubeTimer_API.Models;

public class Discipline
{
    public string Id { get; set; }

    /// <summary>
    /// discipline name 
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}