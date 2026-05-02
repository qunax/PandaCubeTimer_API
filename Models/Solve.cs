namespace PandaCubeTimer_API.Models;

public class Solve
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Session in which this solve was made
    /// </summary>
    public Guid SessionId { get; set; }
    
    /// <summary>
    /// Time in which puzzle was solved
    /// </summary>
    public double SolveTimeSeconds { get; set; }
    
    /// <summary>
    /// Penalty
    /// </summary>
    public bool IsPlusTwo { get; set; }
    
    /// <summary>
    /// Penalty
    /// </summary>
    public bool IsDNF { get; set; }
    
    /// <summary>
    /// How the cube was scrambled before solve
    /// </summary>
    public string Scramble { get; set; } = null!;
    
    /// <summary>
    /// When the puzzleSolve was made
    /// </summary>
    public DateTime DateTime { get; set; }
    
    /// <summary>
    /// Text annotations by user
    /// </summary>
    public string? Comment { get; set; }
    
    public bool IsDeleted { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}