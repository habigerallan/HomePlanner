using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Services.Interfaces;
using System.Security.Claims;
using RoomPlannerAPI.Utilities;

namespace RoomPlannerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController(IAccountService accountService, TokenGenerator tokenGenerator) : ControllerBase
{
    private readonly IAccountService _accountService = accountService;
    private readonly TokenGenerator _tokenGenerator = tokenGenerator;

    [HttpPost("create")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDTO createAccountDTO)
    {
        var account = await _accountService.CreateAccount(createAccountDTO);
        if (account == null)
            return BadRequest("Account creation failed.");

        return Ok(account);
    }

    [HttpDelete("{accountId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAccount(int accountId)
    {
        bool isDeleted = await _accountService.DeleteAccount(accountId);
        if (!isDeleted)
            return NotFound("Account not found.");

        return NoContent();
    }

    [HttpGet("{accountId}")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetAccount(int accountId)
    {
        var account = await _accountService.GetAccount(accountId);
        if (account == null)
            return NotFound("Account not found.");

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userRole == "User" && userIdClaim != account.AccountID.ToString())
            return Forbid("You can only access your own account.");

        return Ok(account);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] CreateAccountDTO loginDTO)
    {
        bool isValid = await _accountService.ValidateAccount(loginDTO.Username, loginDTO.Password);
        if (!isValid)
            return Unauthorized("Invalid username or password.");

        string token = _tokenGenerator.GenerateUserToken(loginDTO.Username);

        return Ok(new { Token = token });
    }
}