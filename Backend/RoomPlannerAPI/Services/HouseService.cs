using Microsoft.Data.SqlClient;
using RoomPlannerAPI.Models;
using RoomPlannerAPI.Services.Interfaces;

namespace RoomPlannerAPI.Services;

public class HouseService(IConfiguration configuration) : IHouseService
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is missing.");

    public async Task<House?> CreateHouse(string name)
    {
        using SqlConnection conn = new(_connectionString);

        string checkQuery = @"
        SELECT COUNT(*) 
        FROM HouseAccount HA
        INNER JOIN Account A ON HA.AccountID = A.AccountID
        WHERE A.name = @name;";

        using SqlCommand checkCmd = new(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@name", name);

        var isAssociated = (int?)await checkCmd.ExecuteScalarAsync() ?? 0;
        if (isAssociated == 0) return null;

        string insertQuery = @"
        INSERT INTO House (Name)
        OUTPUT INSERTED.HouseID, INSERTED.Name
        VALUES (@Name);";

        using SqlCommand insertCmd = new(insertQuery, conn);
        insertCmd.Parameters.AddWithValue("@Name", name);

        using SqlDataReader reader = await insertCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new House
            {
                HouseID = reader.GetInt32(0),
                Name = reader.GetString(1)
            };
        }

        return null;
    }

    public async Task<bool> DeleteHouse(int houseId, string requestingAccountUsername)
    {
        using SqlConnection conn = new(_connectionString);
        await conn.OpenAsync();

        string checkQuery = @"
        SELECT COUNT(*) 
        FROM HouseAccount HA
        INNER JOIN Account A ON HA.AccountID = A.AccountID
        WHERE HA.HouseID = @HouseID AND A.Username = @Username;";

        using SqlCommand checkCmd = new(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@HouseID", houseId);
        checkCmd.Parameters.AddWithValue("@Username", requestingAccountUsername);

        var isAssociated = (int?)await checkCmd.ExecuteScalarAsync() ?? 0;
        if (isAssociated == 0) return false;

        string deleteQuery = "DELETE FROM House WHERE HouseID = @HouseID;";
        using SqlCommand deleteCmd = new(deleteQuery, conn);
        deleteCmd.Parameters.AddWithValue("@HouseID", houseId);

        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<House?> GetHouse(int houseId, string requestingAccountUsername)
    {
        using SqlConnection conn = new(_connectionString);
        await conn.OpenAsync();

        string checkQuery = @"
        SELECT COUNT(*) 
        FROM HouseAccount HA
        INNER JOIN Account A ON HA.AccountID = A.AccountID
        WHERE HA.HouseID = @HouseID AND A.Username = @Username;";

        using SqlCommand checkCmd = new(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@HouseID", houseId);
        checkCmd.Parameters.AddWithValue("@Username", requestingAccountUsername);

        var isAssociated = (int?)await checkCmd.ExecuteScalarAsync() ?? 0;
        if (isAssociated == 0) return null;
        string selectQuery = @"
        SELECT HouseID, Name 
        FROM House 
        WHERE HouseID = @HouseID;";

        using SqlCommand selectCmd = new(selectQuery, conn);
        selectCmd.Parameters.AddWithValue("@HouseID", houseId);

        using SqlDataReader reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new House
            {
                HouseID = reader.GetInt32(0),
                Name = reader.GetString(1)
            };
        }

        return null;
    }

    public async Task<House?> ModifyHouse(int houseId, string name, string requestingAccountUsername)
    {
        using SqlConnection conn = new(_connectionString);
        await conn.OpenAsync();

        string checkQuery = @"
        SELECT COUNT(*) 
        FROM HouseAccount HA
        INNER JOIN Account A ON HA.AccountID = A.AccountID
        WHERE HA.HouseID = @HouseID AND A.Username = @Username;";

        using SqlCommand checkCmd = new(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@HouseID", houseId);
        checkCmd.Parameters.AddWithValue("@Username", requestingAccountUsername);

        var isAssociated = (int?)await checkCmd.ExecuteScalarAsync() ?? 0;
        if (isAssociated == 0) return null;

        string updateQuery = @"
        UPDATE House 
        SET Name = @Name
        WHERE HouseID = @HouseID;
        
        SELECT HouseID, Name FROM House WHERE HouseID = @HouseID;";

        using SqlCommand updateCmd = new(updateQuery, conn);
        updateCmd.Parameters.AddWithValue("@HouseID", houseId);
        updateCmd.Parameters.AddWithValue("@Name", name);

        using SqlDataReader reader = await updateCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new House
            {
                HouseID = reader.GetInt32(0),
                Name = reader.GetString(1)
            };
        }

        return null;
    }

    public async Task<bool> AddUserToHouse(int houseId, string newUser, string requestingUser) {
        using SqlConnection conn = new(_connectionString);
        await conn.OpenAsync();

        string checkQuery = @"
        SELECT COUNT(*) 
        FROM HouseAccount HA
        INNER JOIN Account A ON HA.AccountID = A.AccountID
        WHERE HA.HouseID = @HouseID AND A.Username = @requestingUser;";

        using SqlCommand checkCmd = new(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@HouseID", houseId);
        checkCmd.Parameters.AddWithValue("@requestingUser", requestingUser);

        var isAssociated = (int?)await checkCmd.ExecuteScalarAsync() ?? 0;
        if (isAssociated == 0) return false;

        string checkHouselessQuery = @"
        SELECT COUNT(*) 
        FROM HouseAccount HA
        INNER JOIN Account A ON HA.AccountID = A.AccountID
        WHERE A.Username = @newUser;";

        using SqlCommand checkCmd2 = new(checkHouselessQuery, conn);
        checkCmd2.Parameters.AddWithValue("@newUser", newUser);

        var hasHouse = (int?)await checkCmd2.ExecuteScalarAsync() ?? 0;
        if (hasHouse == 0) return false;

        string updateQuery = @"
        INSERT INTO HouseAccount (Name, HouseID)
        SET Name = @Name
        SET HouseID = @HouseID;";

        using SqlCommand updateCmd = new(updateQuery, conn);
        updateCmd.Parameters.AddWithValue("@HouseID", houseId);
        updateCmd.Parameters.AddWithValue("@Name", name);

        using SqlDataReader reader = await updateCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return true;
        }

        return false;
    }

    public async Task<bool> RemoveUserFromHouse(int houseId, string userToRemove, string requestingUser) {
        using SqlConnection conn = new(_connectionString);
        await conn.OpenAsync();

        string checkQuery = @"
        SELECT COUNT(*) 
        FROM HouseAccount HA
        INNER JOIN Account A ON HA.AccountID = A.AccountID
        WHERE HA.HouseID = @HouseID AND A.Username = @requestingUser;";

        using SqlCommand checkCmd = new(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@HouseID", houseId);
        checkCmd.Parameters.AddWithValue("@requestingUser", requestingUser);

        var isAssociated = (int?)await checkCmd.ExecuteScalarAsync() ?? 0;
        if (isAssociated == 0) return false;

        string checkHouselessQuery = @"
        SELECT COUNT(*) 
        FROM HouseAccount HA
        INNER JOIN Account A ON HA.AccountID = A.AccountID
        WHERE HA.HouseID = @HouseID AND A.Username = @newUser;";

        using SqlCommand checkCmd2 = new(checkHouselessQuery, conn);
        checkCmd.Parameters.AddWithValue("@HouseID", houseId);
        checkCmd2.Parameters.AddWithValue("@newUser", userToRemove);

        var hasHouse = (int?)await checkCmd2.ExecuteScalarAsync() ?? 0;
        if (hasHouse == 0) return false;

        string updateQuery = @"
        DELETE FROM HouseAccount
        WHERE Name = @Name;";

        using SqlCommand updateCmd = new(updateQuery, conn);
        updateCmd.Parameters.AddWithValue("@Name", name);

        using SqlDataReader reader = await updateCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return true;
        }

        return false;
    }

    public async Task<bool> AdminDeleteHouse(int houseId)
    {
        using SqlConnection conn = new(_connectionString);
        await conn.OpenAsync();

        string deleteQuery = "DELETE FROM House WHERE HouseID = @HouseID;";

        using SqlCommand deleteCmd = new(deleteQuery, conn);
        deleteCmd.Parameters.AddWithValue("@HouseID", houseId);

        int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<House?> AdminGetHouse(int houseId)
    {
        using SqlConnection conn = new(_connectionString);
        await conn.OpenAsync();

        string selectQuery = "SELECT HouseID, Name FROM House WHERE HouseID = @HouseID;";

        using SqlCommand selectCmd = new(selectQuery, conn);
        selectCmd.Parameters.AddWithValue("@HouseID", houseId);

        using SqlDataReader reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new House
            {
                HouseID = reader.GetInt32(0),
                Name = reader.GetString(1)
            };
        }

        return null;
    }

    public async Task<House?> AdminModifyHouse(int houseId, string name)
    {
        using SqlConnection conn = new(_connectionString);
        await conn.OpenAsync();

        string updateQuery = @"
        UPDATE House 
        SET Name = @Name
        WHERE HouseID = @HouseID;
        
        SELECT HouseID, Name FROM House WHERE HouseID = @HouseID;";

        using SqlCommand updateCmd = new(updateQuery, conn);
        updateCmd.Parameters.AddWithValue("@HouseID", houseId);
        updateCmd.Parameters.AddWithValue("@Name", name);

        using SqlDataReader reader = await updateCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new House
            {
                HouseID = reader.GetInt32(0),
                Name = reader.GetString(1)
            };
        }

        return null;
    }

}