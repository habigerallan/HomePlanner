using Microsoft.Data.SqlClient;
using Task = RoomPlannerAPI.Models.Task;
using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Services.Interfaces;
using RoomPlannerAPI.Utilities;

namespace RoomPlannerAPI.Services;

public class TaskService(IConfiguration configuration) : ITaskService
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is missing.");

    public async Task<Task?> CreateTask(TaskDTO taskDTO, string requestingAccountUsername)
    {
        using SqlConnection conn = new(_connectionString);

        int? accountId = await TaskUtility.GetAccountIdByUsername(conn, requestingAccountUsername) ?? throw new InvalidOperationException("Requesting account does not exist.");
        bool hasHouse = await TaskUtility.UserHasHouse(conn, accountId.Value);
        if (!hasHouse)
        {
            throw new InvalidOperationException("User must be associated with a house to create a task.");
        }

        const string query = @"
                INSERT INTO Task (Name, Description, Due, Complete)
                OUTPUT INSERTED.TaskID, INSERTED.Name, INSERTED.Description, INSERTED.Due, INSERTED.Complete
                VALUES (@Name, @Description, @Due, @Complete);";

        using var command = new SqlCommand(query, conn);
        command.Parameters.AddWithValue("@Name", taskDTO.Name);
        command.Parameters.AddWithValue("@Description", (object?)taskDTO.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Due", taskDTO.Due == default ? DBNull.Value : taskDTO.Due);
        command.Parameters.AddWithValue("@Complete", taskDTO.Complete);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Task
            {
                TaskID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Due = reader.GetDateTime(3),
                Complete = reader.GetBoolean(4)
            };
        }

        return null;
    }

    public async Task<bool> DeleteTask(int taskId, string requestingAccountUsername)
    {
        using SqlConnection conn = new(_connectionString);

        int? accountId = await TaskUtility.GetAccountIdByUsername(conn, requestingAccountUsername) ?? throw new InvalidOperationException("Requesting account does not exist.");
        bool isUserAuthorized = await TaskUtility.CanUserInteractWithTask(conn, accountId.Value, taskId);
        if (!isUserAuthorized)
        {
            throw new UnauthorizedAccessException("User is not authorized to delete this task.");
        }

        const string query = "DELETE FROM Task WHERE TaskID = @TaskID;";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@TaskID", taskId);

        await conn.OpenAsync();
        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<Task?> GetTask(int taskId, string requestingAccountUsername)
    {
        using SqlConnection conn = new(_connectionString);

        int? accountId = await TaskUtility.GetAccountIdByUsername(conn, requestingAccountUsername) ?? throw new InvalidOperationException("Requesting account does not exist.");
        bool isUserAuthorized = await TaskUtility.CanUserInteractWithTask(conn, accountId.Value, taskId);
        if (!isUserAuthorized)
        {
            throw new UnauthorizedAccessException("User is not authorized to view this task.");
        }

        const string query = @"
        SELECT TaskID, Name, Description, Due, Complete 
        FROM Task 
        WHERE TaskID = @TaskID;";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@TaskID", taskId);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Task
            {
                TaskID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Due = reader.GetDateTime(3),
                Complete = reader.GetBoolean(4)
            };
        }

        return null;
    }

    public async Task<Task?> ModifyTask(int taskId, TaskDTO taskDTO, string requestingAccountUsername)
    {
        using SqlConnection conn = new(_connectionString);
        await conn.OpenAsync();

        int? accountId = await TaskUtility.GetAccountIdByUsername(conn, requestingAccountUsername) ?? throw new InvalidOperationException("Requesting account does not exist.");
        bool isUserAuthorized = await TaskUtility.CanUserInteractWithTask(conn, accountId.Value, taskId);
        if (!isUserAuthorized)
        {
            throw new UnauthorizedAccessException("User is not authorized to modify this task.");
        }

        const string query = @"
        UPDATE Task 
        SET Name = @Name, Description = @Description, Due = @Due, Complete = @Complete
        WHERE TaskID = @TaskID;
        SELECT TaskID, Name, Description, Due, Complete FROM Task WHERE TaskID = @TaskID;";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@TaskID", taskId);
        cmd.Parameters.AddWithValue("@Name", taskDTO.Name);
        cmd.Parameters.AddWithValue("@Description", (object?)taskDTO.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Due", taskDTO.Due == default ? DBNull.Value : taskDTO.Due);
        cmd.Parameters.AddWithValue("@Complete", taskDTO.Complete);

        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var updatedTask = new Task
            {
                TaskID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Due = reader.GetDateTime(3),
                Complete = reader.GetBoolean(4)
            };

            return updatedTask;
        }

        return null;
    }

    public async Task<Task?> AdminCreateTask(TaskDTO taskDTO)
    {
        using SqlConnection conn = new(_connectionString);
        string insertQuery = @"
        INSERT INTO Task (Name, Description, Due, Complete)
        OUTPUT INSERTED.TaskID, INSERTED.Name, INSERTED.Description, INSERTED.Due, INSERTED.Complete
        VALUES (@Name, @Description, @Due, @Complete);";

        using SqlCommand insertCmd = new(insertQuery, conn);
        insertCmd.Parameters.AddWithValue("@Name", taskDTO.Name);
        insertCmd.Parameters.AddWithValue("@Description", taskDTO.Description);
        insertCmd.Parameters.AddWithValue("@Due", taskDTO.Due);
        insertCmd.Parameters.AddWithValue("@Complete", taskDTO.Complete);

        await conn.OpenAsync();
        using SqlDataReader reader = await insertCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Task
            {
                TaskID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Due = reader.GetDateTime(3),
                Complete = reader.GetBoolean(4)
            };
        }

        return null;
    }

    public async Task<bool> AdminDeleteTask(int taskId)
    {
        using SqlConnection conn = new(_connectionString);
        string query = "DELETE FROM Task WHERE TaskID = @TaskID;";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@TaskID", taskId);

        await conn.OpenAsync();
        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<Task?> AdminGetTask(int taskId)
    {
        using SqlConnection conn = new(_connectionString);
        string query = @"
        SELECT TaskID, Name, Description, Due, Complete
        FROM Task
        WHERE TaskID = @TaskID;";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@TaskID", taskId);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Task
            {
                TaskID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Due = reader.GetDateTime(3),
                Complete = reader.GetBoolean(4)
            };
        }

        return null;
    }

    public async Task<Task?> AdminModifyTask(int taskId, TaskDTO taskDTO)
    {
        using SqlConnection conn = new(_connectionString);
        string updateQuery = @"
        UPDATE Task
        SET Name = @Name, Description = @Description, Due = @Due, Complete = @Complete
        WHERE TaskID = @TaskID;
        SELECT TaskID, Name, Description, Due, Complete FROM Task WHERE TaskID = @TaskID;";

        using SqlCommand updateCmd = new(updateQuery, conn);
        updateCmd.Parameters.AddWithValue("@TaskID", taskId);
        updateCmd.Parameters.AddWithValue("@Name", taskDTO.Name);
        updateCmd.Parameters.AddWithValue("@Description", taskDTO.Description);
        updateCmd.Parameters.AddWithValue("@Due", taskDTO.Due);
        updateCmd.Parameters.AddWithValue("@Complete", taskDTO.Complete);

        await conn.OpenAsync();
        using SqlDataReader reader = await updateCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Task
            {
                TaskID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Due = reader.GetDateTime(3),
                Complete = reader.GetBoolean(4)
            };
        }

        return null;
    }
}