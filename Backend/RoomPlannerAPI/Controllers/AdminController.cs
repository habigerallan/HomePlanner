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
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAccountDTO createAccountDTO)
    {
        var admin = await _adminService.CreateAdmin(createAccountDTO);
        if (admin == null)
            return BadRequest("Account creation failed.");

        return Ok(admin);
    }

    [HttpDelete("{adminId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAdmin(int adminId)
    {
        bool isDeleted = await _adminService.DeleteAdmin(adminId);
        if (!isDeleted)
            return NotFound("Account not found.");

        return NoContent();
    }

    [HttpGet("{adminId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdmin(int adminId)
    {
        var admin = await _adminService.GetAdmin(adminId);
        if (admin == null)
            return NotFound("Admin not found.");

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userRole == "User" && userIdClaim != admin.AdminID.ToString())
            return Forbid("You can only access your own admin.");

        return Ok(admin);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] CreateAccountDTO loginDTO)
    {
        bool isValid = await _adminService.ValidateAdmin(loginDTO.Username, loginDTO.Password);
        if (!isValid)
            return Unauthorized("Invalid username or password.");

        string token = _tokenGenerator.GenerateAdminToken(loginDTO.Username);

        return Ok(new { Token = token });
    }
}