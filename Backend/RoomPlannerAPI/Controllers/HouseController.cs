using RoomPlannerAPI.Models;
using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Services.Interfaces;
using RoomPlannerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RoomPlannerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HouseController(IHouseService houseService) : ControllerBase
{
    private readonly IHouseService _houseService = houseService;

    private bool UserHasRole(string role)
    {
        return HttpContext.User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role);
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateHouse([FromBody] HouseDTO houseRequest)
    {
        if (houseRequest == null || string.IsNullOrWhiteSpace(houseRequest.Name))
        {
            return BadRequest("Name is required.");
        }

        var house = await _houseService.CreateHouse(houseRequest.Name);

        if (house == null)
            return BadRequest("House creation failed.");

        return Ok(house);
    }

    [HttpDelete("{houseId}")]
    [Authorize]
    public async Task<IActionResult> DeleteHouse(int houseId)
    {
        var accountUsername = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        bool isDeleted = UserHasRole("Admin")
            ? await _houseService.AdminDeleteHouse(houseId)
            : await _houseService.DeleteHouse(houseId, accountUsername);

        if (!isDeleted)
            return NotFound("House not found or insufficient permissions.");

        return NoContent();
    }

    [HttpGet("{houseId}")]
    [Authorize]
    public async Task<IActionResult> GetHouse(int houseId)
    {
        var accountUsername = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        var house = UserHasRole("Admin")
            ? await _houseService.AdminGetHouse(houseId)
            : await _houseService.GetHouse(houseId, accountUsername);

        if (house == null)
            return NotFound("House not found.");

        return Ok(house);
    }

    [HttpPut("modify/{houseId}")]
    [Authorize]
    public async Task<IActionResult> ModifyHouse(int houseId, [FromBody] HouseDTO houseRequest)
    {

        var accountUsername = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        var modifiedHouse = UserHasRole("Admin")
            ? await _houseService.AdminModifyHouse(houseId, houseRequest.Name)
            : await _houseService.ModifyHouse(houseId, houseRequest.Name, accountUsername);

        if (modifiedHouse == null)
            return BadRequest("House modification failed.");

        return Ok(modifiedHouse);
    }
}
