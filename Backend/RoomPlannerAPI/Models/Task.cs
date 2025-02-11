namespace RoomPlannerAPI.Models;

public class Task
{
    public int TaskID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Due { get; set; }
    public bool Complete { get; set; }
}
