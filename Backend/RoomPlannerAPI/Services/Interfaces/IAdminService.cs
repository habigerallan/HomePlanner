using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Models;

namespace RoomPlannerAPI.Services.Interfaces;
public interface IAdminService
{
    Task<Admin?> Create(CreateAdminDTO createAdminDTO);
    Task<Admin?> Read(ReadAdminDTO readAdminDTO);
    Task<Admin?> Update(UpdateAdminDTO updateAdminDTO);
    Task<bool> Delete(DeleteAdminDTO deleteAdminDTO);

    Task<int> Login(LoginAdminDTO loginAdminDTO);
}
