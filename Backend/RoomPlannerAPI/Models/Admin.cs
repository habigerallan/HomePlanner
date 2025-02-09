namespace RoomPlannerAPI.Models;

public class Admin
{
    public int AdminID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public int? ProfileID { get; set; }
}

