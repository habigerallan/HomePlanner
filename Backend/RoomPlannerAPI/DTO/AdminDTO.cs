namespace RoomPlannerAPI.DTO;

public record CreateAdminDTO(string Username, string Password);
public record ReadAdminDTO(int AdminID);
public record UpdateAdminDTO(string Username, int ProfileID);
public record DeleteAdminDTO(int AdminID);

public record LoginAdminDTO(string Username, string Password);
