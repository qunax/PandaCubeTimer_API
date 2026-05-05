namespace PandaCubeTimer_API.Models;

public class Session
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } 
    public string Name { get; set; } = string.Empty;
    
    public string DisciplineId { get; set; }
    public Discipline Discipline { get; set; } = null!; 
    public bool IsDeleted { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Solve> Solves { get; set; } = new List<Solve>();
}