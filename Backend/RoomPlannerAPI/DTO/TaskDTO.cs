namespace RoomPlannerAPI.DTO;

public record TaskDTO(string Name, string Description, DateTime Due, bool Complete);
