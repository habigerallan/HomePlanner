using RoomPlannerAPI.Models;

namespace RoomPlannerAPI.Services.Interfaces;
public interface IHouseService
{
    Task<House?> CreateHouse(string name);
    Task<bool> DeleteHouse(int houseId, string requestingAccountUsername);
    Task<House?> GetHouse(int houseId, string requestingAccountUsername);
    Task<House?> ModifyHouse(int houseId, string name, string requestingAccountUsername);

    Task<bool> AddUserToHouse(int houseId, string newUser, string requestingUser);
    Task<bool> RemoveUserFromHouse(int houseId, string userToRemove, string requestingUser);

    Task<bool> AdminDeleteHouse(int houseId);
    Task<House?> AdminGetHouse(int houseId);
    Task<House?> AdminModifyHouse(int houseId, string name);

    Task<bool> AdminAddUserToHouse(int houseId, string newUser);
    Task<bool> AdminRemoveUserFromHouse(int houseId, string userToRemove);

}
