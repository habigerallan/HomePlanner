using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Models;
using RoomPlannerAPI.Services.Interfaces;

namespace RoomPlannerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProfileController(IProfileService profileService) : ControllerBase
{
    private readonly IProfileService _profileService = profileService;

    private bool UserHasRole(string role)
    {
        return HttpContext.User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role);
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateProfile([FromBody] Profile profileRequest)
    {
        var accountUsername = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        if (profileRequest == null ||
            string.IsNullOrWhiteSpace(profileRequest.FirstName) ||
            string.IsNullOrWhiteSpace(profileRequest.LastName) ||
            string.IsNullOrWhiteSpace(profileRequest.PreferredName) ||
            string.IsNullOrWhiteSpace(profileRequest.PhoneNumber) ||
            string.IsNullOrWhiteSpace(profileRequest.Email))
        {
            return BadRequest("First name, last name, phone number, and email are required.");
        }

        var profile = UserHasRole("Admin")
            ? await _profileService.AdminCreateProfile(
                profileRequest.FirstName,
                profileRequest.LastName,
                profileRequest.PreferredName,
                profileRequest.PhoneNumber,
                profileRequest.Email
            )
            : await _profileService.CreateProfile(
                accountUsername,
                profileRequest.FirstName,
                profileRequest.LastName,
                profileRequest.PreferredName,
                profileRequest.PhoneNumber,
                profileRequest.Email
            );

        if (profile == null)
            return BadRequest("Profile creation failed.");

        return Ok(profile);
    }

    [HttpDelete("{profileId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProfile(int profileId)
    {
        bool isDeleted = await _profileService.DeleteProfile(profileId);
        if (!isDeleted)
            return NotFound("Profile not found.");

        return NoContent();
    }

    [HttpGet("{profileId}")]
    [Authorize]
    public async Task<IActionResult> GetProfile(int profileId)
    {
        var accountUsername = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        var profile = UserHasRole("Admin")
            ? await _profileService.AdminGetProfile(profileId)
            : await _profileService.GetProfile(profileId, accountUsername);

        if (profile == null)
            return NotFound("Profile not found.");

        return Ok(profile);
    }

    [HttpPut("modify/{profileId}")]
    [Authorize]
    public async Task<IActionResult> ModifyProfile(int profileId, [FromBody] ModifyProfileDTO profileRequest)
    {
        var accountUsername = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var profile = new Profile
        {
            ProfileID = profileId,
            FirstName = profileRequest.FirstName,
            LastName = profileRequest.LastName,
            PreferredName = profileRequest.PreferredName,
            PhoneNumber = profileRequest.PhoneNumber,
            Email = profileRequest.Email
        };

        var modifiedProfile = UserHasRole("Admin")
            ? await _profileService.AdminModifyProfile(profile)
            : await _profileService.ModifyProfile(accountUsername, profile);

        if (modifiedProfile == null)
            return BadRequest("Profile modification failed.");

        return Ok(modifiedProfile);
    }
}
