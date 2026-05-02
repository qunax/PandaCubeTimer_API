namespace PandaCubeTimer_API.Models;

public class SolvesSyncRequest
{
    // Время последней успешной синхронизации на этом устройстве (в UTC!)
    public DateTime? LastSyncTime { get; set; } 
    
    // ID сессий, которые есть и на телефоне, и на сервере ("Уже связаны")
    public List<Guid> LinkedSessionIds { get; set; } = new(); 
    
    // Новые сборки или "надгробия" (IsDeleted = true), созданные оффлайн
    public List<Solve> LocalChanges { get; set; } = new(); 
}