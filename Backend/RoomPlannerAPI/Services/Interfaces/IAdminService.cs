using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Models;

namespace RoomPlannerAPI.Services.Interfaces;
public interface IAdminService
{
    Task<Admin?> CreateAdmin(CreateAccountDTO createAccountDTO);
    Task<bool> DeleteAdmin(int adminId);
    Task<Admin?> GetAdmin(int adminId);
    Task<bool> ValidateAdmin(string username, string password);
}