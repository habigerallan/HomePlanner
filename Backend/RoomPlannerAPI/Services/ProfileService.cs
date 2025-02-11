using Microsoft.Data.SqlClient;
using RoomPlannerAPI.Models;
using RoomPlannerAPI.Services.Interfaces;

namespace RoomPlannerAPI.Services;

public class ProfileService(IConfiguration configuration) : IProfileService
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is missing.");

    public async Task<Profile?> CreateProfile(string accountUsername, string firstName, string lastName, string preferredName, string phoneNumber, string email)
    {
        using SqlConnection conn = new(_connectionString);
        string checkQuery = "SELECT ProfileID FROM Account WHERE Username = @Username AND ProfileID IS NOT NULL;";

        using SqlCommand checkCmd = new(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@Username", accountUsername);

        await conn.OpenAsync();
        var existingProfile = await checkCmd.ExecuteScalarAsync();
        if (existingProfile != null) return null;

        string insertQuery = @"
            INSERT INTO Profile (FirstName, LastName, PreferredName, PhoneNumber, Email)
            OUTPUT INSERTED.ProfileID, INSERTED.FirstName, INSERTED.LastName, INSERTED.PreferredName, INSERTED.PhoneNumber, INSERTED.Email
            VALUES (@FirstName, @LastName, @PreferredName, @PhoneNumber, @Email);";

        using SqlCommand insertCmd = new(insertQuery, conn);
        insertCmd.Parameters.AddWithValue("@FirstName", firstName);
        insertCmd.Parameters.AddWithValue("@LastName", lastName);
        insertCmd.Parameters.AddWithValue("@PreferredName", preferredName);
        insertCmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
        insertCmd.Parameters.AddWithValue("@Email", email);

        using SqlDataReader reader = await insertCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Profile
            {
                ProfileID = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                PreferredName = reader.GetString(3),
                PhoneNumber = reader.GetString(4),
                Email = reader.GetString(5)
            };
        }

        return null;
    }

    public async Task<bool> DeleteProfile(int profileId)
    {
        using SqlConnection conn = new(_connectionString);
        string query = "DELETE FROM Profile WHERE ProfileID = @ProfileID;";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@ProfileID", profileId);

        await conn.OpenAsync();
        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<Profile?> GetProfile(int profileId, string requestingAccountUsername)
    {
        using SqlConnection conn = new(_connectionString);
        string query = @"
            SELECT p.ProfileID, p.FirstName, p.LastName, p.PreferredName, p.PhoneNumber, p.Email
            FROM Profile p
            JOIN Account a ON p.ProfileID = a.ProfileID
            JOIN HouseAccount ha ON a.Username = ha.Username
            WHERE p.ProfileID = @ProfileID
            AND ha.HouseID IN (SELECT HouseID FROM HouseAccount WHERE Username = @RequestingUsername);";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@ProfileID", profileId);
        cmd.Parameters.AddWithValue("@RequestingUsername", requestingAccountUsername);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Profile
            {
                ProfileID = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                PreferredName = reader.GetString(3),
                PhoneNumber = reader.GetString(4),
                Email = reader.GetString(5)
            };
        }

        return null;
    }

    public async Task<Profile?> ModifyProfile(string requestingAccountUsername, Profile profile)
    {
        using SqlConnection conn = new(_connectionString);
        string query = "SELECT ProfileID FROM Account WHERE Username = @RequestingUsername AND ProfileID = @ProfileID;";

        using SqlCommand checkCmd = new(query, conn);
        checkCmd.Parameters.AddWithValue("@RequestingUsername", requestingAccountUsername);
        checkCmd.Parameters.AddWithValue("@ProfileID", profile.ProfileID);

        await conn.OpenAsync();
        var result = await checkCmd.ExecuteScalarAsync();
        if (result == null) return null;

        string updateQuery = @"
            UPDATE Profile
            SET FirstName = @FirstName, LastName = @LastName, PreferredName = @PreferredName, 
                PhoneNumber = @PhoneNumber, Email = @Email
            WHERE ProfileID = @ProfileID;";

        using SqlCommand updateCmd = new(updateQuery, conn);
        updateCmd.Parameters.AddWithValue("@FirstName", profile.FirstName);
        updateCmd.Parameters.AddWithValue("@LastName", profile.LastName);
        updateCmd.Parameters.AddWithValue("@PreferredName", profile.PreferredName);
        updateCmd.Parameters.AddWithValue("@PhoneNumber", profile.PhoneNumber);
        updateCmd.Parameters.AddWithValue("@Email", profile.Email);
        updateCmd.Parameters.AddWithValue("@ProfileID", profile.ProfileID);

        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
        return rowsAffected > 0 ? profile : null;
    }

    public async Task<Profile?> AdminCreateProfile(string firstName, string lastName, string preferredName, string phoneNumber, string email)
    {
        using SqlConnection conn = new(_connectionString);
        string insertQuery = @"
        INSERT INTO Profile (FirstName, LastName, PreferredName, PhoneNumber, Email)
        OUTPUT INSERTED.ProfileID, INSERTED.FirstName, INSERTED.LastName, INSERTED.PreferredName, INSERTED.PhoneNumber, INSERTED.Email
        VALUES (@FirstName, @LastName, @PreferredName, @PhoneNumber, @Email);";

        using SqlCommand insertCmd = new(insertQuery, conn);
        insertCmd.Parameters.AddWithValue("@FirstName", firstName);
        insertCmd.Parameters.AddWithValue("@LastName", lastName);
        insertCmd.Parameters.AddWithValue("@PreferredName", preferredName);
        insertCmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
        insertCmd.Parameters.AddWithValue("@Email", email);

        await conn.OpenAsync();
        using SqlDataReader reader = await insertCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Profile
            {
                ProfileID = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                PreferredName = reader.GetString(3),
                PhoneNumber = reader.GetString(4),
                Email = reader.GetString(5)
            };
        }

        return null;
    }

    public async Task<bool> AdminDeleteProfile(int profileId)
    {
        using SqlConnection conn = new(_connectionString);
        string query = "DELETE FROM Profile WHERE ProfileID = @ProfileID;";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@ProfileID", profileId);

        await conn.OpenAsync();
        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<Profile?> AdminGetProfile(int profileId)
    {
        using SqlConnection conn = new(_connectionString);
        string query = @"
        SELECT ProfileID, FirstName, LastName, PreferredName, PhoneNumber, Email
        FROM Profile
        WHERE ProfileID = @ProfileID;";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@ProfileID", profileId);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Profile
            {
                ProfileID = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                PreferredName = reader.GetString(3),
                PhoneNumber = reader.GetString(4),
                Email = reader.GetString(5)
            };
        }

        return null;
    }

    public async Task<Profile?> AdminModifyProfile(Profile profile)
    {
        using SqlConnection conn = new(_connectionString);
        string updateQuery = @"
        UPDATE Profile
        SET FirstName = @FirstName, LastName = @LastName, PreferredName = @PreferredName, 
            PhoneNumber = @PhoneNumber, Email = @Email
        WHERE ProfileID = @ProfileID;";

        using SqlCommand updateCmd = new(updateQuery, conn);
        updateCmd.Parameters.AddWithValue("@FirstName", profile.FirstName);
        updateCmd.Parameters.AddWithValue("@LastName", profile.LastName);
        updateCmd.Parameters.AddWithValue("@PreferredName", profile.PreferredName);
        updateCmd.Parameters.AddWithValue("@PhoneNumber", profile.PhoneNumber);
        updateCmd.Parameters.AddWithValue("@Email", profile.Email);
        updateCmd.Parameters.AddWithValue("@ProfileID", profile.ProfileID);

        await conn.OpenAsync();
        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
        return rowsAffected > 0 ? profile : null;
    }
}
