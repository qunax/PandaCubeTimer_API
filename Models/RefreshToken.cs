namespace PandaCubeTimer_API.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; } 
    public string Token { get; set; } = string.Empty; // Сам длинный случайный токен
    public DateTime ExpiresAt { get; set; } // Когда он протухнет (например, через 60 дней)
    public bool IsRevoked { get; set; } = false; // Был ли он принудительно отозван (для кнопки "Выйти со всех устройств")
    
    // Бонус: Имя устройства, чтобы показывать пользователю список (как в Telegram)
    public string DeviceName { get; set; } = string.Empty; 
}