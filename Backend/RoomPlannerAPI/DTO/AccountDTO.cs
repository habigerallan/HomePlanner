namespace RoomPlannerAPI.DTO;

public record CreateAccountDTO(string Username, string Password);
public record ReadAccountDTO(int AccountID);
public record UpdateAccountDTO(string Username, int ProfileID);
public record DeleteAccountDTO(int AccountID);

public record LoginAccountDTO(string Username, string Password);
