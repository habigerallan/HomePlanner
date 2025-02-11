using Microsoft.Data.SqlClient;

namespace RoomPlannerAPI.Utilities;

public static class TaskUtility
{
    public static async Task<bool> CanUserInteractWithTask(SqlConnection connection, int accountId, int taskId)
    {
        const string query = @"
        SELECT TOP 1 1 
        FROM HouseAccount ha
        JOIN HouseTask ht ON ha.HouseID = ht.HouseID
        WHERE ha.AccountID = @AccountID AND ht.TaskID = @TaskID";

        using SqlCommand cmd = new(query, connection);
        cmd.Parameters.AddWithValue("@AccountID", accountId);
        cmd.Parameters.AddWithValue("@TaskID", taskId);

        await connection.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }

    public static async Task<int?> GetAccountIdByUsername(SqlConnection connection, string username)
    {
        const string query = "SELECT AccountID FROM Account WHERE Username = @Username";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);
        var result = await command.ExecuteScalarAsync();
        return result == null ? null : (int?)result;
    }

    public static async Task<bool> UserHasHouse(SqlConnection connection, int accountId)
    {
        const string query = "SELECT TOP 1 1 FROM HouseAccount WHERE AccountID = @AccountID";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AccountID", accountId);
        var result = await command.ExecuteScalarAsync();
        return result != null;
    }
}