using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Models;

namespace RoomPlannerAPI.Services.Interfaces;
public interface IAccountService
{
    Task<Account?> CreateAccount(CreateAccountDTO createAccountDTO);
    Task<bool> DeleteAccount(int accountId);
    Task<Account?> GetAccount(int accountId);
    Task<bool> ValidateAccount(string username, string password);
}