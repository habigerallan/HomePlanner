using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Services.Interfaces;
using System.Security.Claims;
using RoomPlannerAPI.Utilities;

namespace RoomPlannerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController(IAdminService adminService, TokenGenerator tokenGenerator) : ControllerBase
{
    private readonly IAdminService _adminService = adminService;
    private readonly TokenGenerator _tokenGenerator = tokenGenerator;

    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDTO createAdminDTO)
    {
        Admin admin = await _adminService.CreateAdmin(createAdminDTO);

        if (!admin)
        {
            return BadRequest("Admin creation failed.");
        }

        return Ok(admin);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReadAdmin(ReadAdminDTO readAdminDTO)
    {
        var admin = await _adminService.GetAdmin(readAdminDTO);

        if (!admin)
        {
            return NotFound("Admin not found.");
        }

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userRole == "User" && userIdClaim != admin.AdminID.ToString())
            return Forbid("You can only access your own admin.");

        return Ok(admin);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAdmin(int adminId)
    {
        bool isDeleted = await _adminService.DeleteAdmin(adminId);
        if (!isDeleted)
            return NotFound("Admin not found.");

        return NoContent();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAdmin([FromBody] CreateAccountDTO loginDTO)
    {
        int userID = await _adminService.Login(loginDTO);

        if (!userID)
        {
            return Unauthorized("Invalid username or password.");
        }

        string token = _tokenGenerator.GenerateAdminToken(loginDTO.Username);

        return Ok({ Token = token });
    }
}
