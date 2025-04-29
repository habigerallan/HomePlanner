using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Services.Interfaces;
using System.Security.Claims;
using RoomPlannerAPI.Utilities;

namespace RoomPlannerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController(IAccountService accountService, IProfileService profileService, TokenGenerator tokenGenerator) : ControllerBase
{

    private readonly IAccountService _accountService = accountService;
    private readonly TokenGenerator _tokenGenerator = tokenGenerator;

    [HttpPost("create")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDTO createAccountDTO)
    {
        Account account = await _accountService.Create(createAccountDTO);

        if (!account) 
        {
            return BadRequest("Account creation failed.");
        }

        return Ok(account);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> ReadAccount([FromBody] ReadAccountDTO readAccountDTO)
    {
        string role = User.FindFirst(ClaimTypes.Role).Value;
        int accountID = User.FindFirst(ClaimTypes.Sid).Value;
        string username = User.FindFirst(ClaimTypes.NameIdentifier).Value;

        Account? account = null;

        if (role == "User") 
        {
            if (readAccountDTO.AccountID != requestAccountID) 
            {
                return NotAuthorized("Account not authorized.");
            }

            account = await _accountService.Read(readAccountDTO);

            if (!account)
            {
                return NotFound("Account not found.");
            }
        } 
        
        else
        {
            account = await _accountService.Read(readAccountDTO);

            if (!account)
            {
                return NotFound("Account not found.");
            }
        }

        return Ok(account);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountDTO updateAccountDTO)
    {
        string role = User.FindFirst(ClaimTypes.Role).Value;
        int accountID = User.FindFirst(ClaimTypes.Sid).Value;
        string username = User.FindFirst(ClaimTypes.NameIdentifier).Value;

        Account? account = null;

        if (role == "User") 
        {
            if (updateAccountDTO.AccountID != requestAccountID) 
            {
                return NotAuthorized("Account not authorized.");
            }

            account = await _accountService.Update(updateAccountDTO);

            if (!account)
            {
                return NotFound("Account not found.");
            }
        } 
        
        else
        {
            account = await _accountService.Update(updateAccountDTO);

            if (!account)
            {
                return NotFound("Account not found.");
            }
        }

        return Ok(account);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDTO deleteAccountDTO)
    {
        string role = User.FindFirst(ClaimTypes.Role).Value;
        int accountID = User.FindFirst(ClaimTypes.Sid).Value;
        string username = User.FindFirst(ClaimTypes.NameIdentifier).Value;

        bool isDeleted = false;

        if (role == "User")
        {
            if (accountID != deleteAccountDTO.AccountID) 
            {
                return NotAuthorized("Account not authorized.");
            }

            isDeleted = await _accountService.Delete(deleteAccountDTO);

            if (!isDeleted)
            {
                return NotFound("Account not found.");
            }
        }

        else
        {
            isDeleted = await _accountService.Delete(deleteAccountDTO);

            if (!isDeleted)
            {
                return NotFound("Account not found.");
            }
        }

        return Ok(isDeleted);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAccount([FromBody] LoginAccountDTO loginDTO)
    {
        int userID = await _accountService.Login(loginDTO.Username, loginDTO.Password);

        if (!userID)
        {
            return Unauthorized("Invalid username or password.");
        }

        string token = _tokenGenerator.GenerateUserToken(loginDTO.Username, userID);

        return Ok({ Token = token });
    }
}
