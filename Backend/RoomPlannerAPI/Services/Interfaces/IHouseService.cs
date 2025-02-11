using RoomPlannerAPI.Models;

namespace RoomPlannerAPI.Services.Interfaces;
public interface IHouseService
{
    Task<House?> CreateHouse(string name);
    Task<bool> DeleteHouse(int houseId, string requestingAccountUsername);
    Task<House?> GetHouse(int houseId, string requestingAccountUsername);
    Task<House?> ModifyHouse(int houseId, string name, string requestingAccountUsername);

    Task<bool> AdminDeleteHouse(int houseId);
    Task<House?> AdminGetHouse(int houseId);
    Task<House?> AdminModifyHouse(int houseId, string name);

}
