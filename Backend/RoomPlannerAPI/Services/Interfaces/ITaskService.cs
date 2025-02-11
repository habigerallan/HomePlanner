using Task = RoomPlannerAPI.Models.Task;
using RoomPlannerAPI.DTO;

namespace RoomPlannerAPI.Services.Interfaces;
public interface ITaskService
{
    Task<Task?> CreateTask(TaskDTO taskDTO, string requestingAccountUsername);
    Task<bool> DeleteTask(int taskId, string requestingAccountUsername);
    Task<Task?> GetTask(int taskId, string requestingAccountUsername);
    Task<Task?> ModifyTask(int taskId, TaskDTO taskDTO, string requestingAccountUsername);

    Task<Task?> AdminCreateTask(TaskDTO taskDTO);
    Task<bool> AdminDeleteTask(int taskId);
    Task<Task?> AdminGetTask(int taskId);
    Task<Task?> AdminModifyTask(int taskId, TaskDTO taskDTO);

}
