using Task = RoomPlannerAPI.Models.Task;
using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RoomPlannerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskController(ITaskService taskService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService;

    private bool UserHasRole(string role)
    {
        return HttpContext.User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role);
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateTask([FromBody] TaskDTO taskRequest)
    {
        if (taskRequest == null ||
            string.IsNullOrWhiteSpace(taskRequest.Name) ||
            string.IsNullOrWhiteSpace(taskRequest.Description) ||
            taskRequest.Due == default)
        {
            return BadRequest("Task name, description, and a valid due date are required.");
        }

        var accountUsername = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        Task? task = UserHasRole("Admin")
            ? await _taskService.AdminCreateTask(taskRequest)
            : await _taskService.CreateTask(taskRequest, accountUsername);

        if (task == null)
            return BadRequest("Task creation failed.");

        return Ok(task);
    }

    [HttpDelete("{taskId}")]
    [Authorize]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        var accountUsername = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        bool isDeleted = UserHasRole("Admin")
            ? await _taskService.AdminDeleteTask(taskId)
            : await _taskService.DeleteTask(taskId, accountUsername);

        if (!isDeleted)
            return NotFound("Task not found or insufficient permissions.");

        return NoContent();
    }

    [HttpGet("{taskId}")]
    [Authorize]
    public async Task<IActionResult> GetTask(int taskId)
    {
        var accountUsername = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        var task = UserHasRole("Admin")
            ? await _taskService.AdminGetTask(taskId)
            : await _taskService.GetTask(taskId, accountUsername);

        if (task == null)
            return NotFound("Task not found.");

        return Ok(task);
    }

    [HttpPut("modify/{taskId}")]
    [Authorize]
    public async Task<IActionResult> ModifyTask(int taskId, [FromBody] TaskDTO taskRequest)
    {
        var accountUsername = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        var modifiedTask = UserHasRole("Admin")
            ? await _taskService.AdminModifyTask(taskId, taskRequest)
            : await _taskService.ModifyTask(taskId, taskRequest, accountUsername);

        if (modifiedTask == null)
            return BadRequest("Task modification failed.");

        return Ok(modifiedTask);
    }
}
