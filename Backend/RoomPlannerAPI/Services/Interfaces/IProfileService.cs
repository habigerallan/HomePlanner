using RoomPlannerAPI.Models;

namespace RoomPlannerAPI.Services.Interfaces;
public interface IProfileService
{
    Task<Profile?> CreateProfile(string accountUsername, string firstName, string lastName, string preferredName, string phoneNumber, string email);
    Task<bool> DeleteProfile(int profileId);
    Task<Profile?> GetProfile(int profileId, string requestingAccountUsername);
    Task<Profile?> ModifyProfile(string requestingAccountUsername, Profile profile);

    Task<Profile?> AdminCreateProfile(string firstName, string lastName, string preferredName, string phoneNumber, string email);
    Task<bool> AdminDeleteProfile(int profileId);
    Task<Profile?> AdminGetProfile(int profileId);
    Task<Profile?> AdminModifyProfile(Profile profile);
}
