using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Models;

namespace RoomPlannerAPI.Services.Interfaces;
public interface IAccountService
{
    Task<Account?> Create(CreateAccountDTO createAccountDTO);
    Task<Account?> Read(ReadAccountDTO readAccountDTO);
    Task<Account?> Update(UpdateAccountDTO updateAccountDTO);
    Task<bool> Delete(DeleteAccountDTO deleteAccountDTO);

    Task<int> Login(LoginAccountDTO loginAccountDTO);
}
