using Microsoft.Data.SqlClient;
using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Models;
using RoomPlannerAPI.Utilities;
using RoomPlannerAPI.Services.Interfaces;

namespace RoomPlannerAPI.Services;

public class AdminService(IConfiguration configuration) : IAdminService
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is missing.");

    public async Task<Admin?> CreateAdmin(CreateAccountDTO createAccountDTO)
    {
        var (hashedPassword, salt) = PasswordHasher.HashPassword(createAccountDTO.Password);

        using SqlConnection conn = new(_connectionString);
        string query = @"
                    INSERT INTO Admin (Username, PasswordHash, PasswordSalt) 
                    OUTPUT INSERTED.AdminID, INSERTED.Username, INSERTED.PasswordHash, INSERTED.PasswordSalt, INSERTED.ProfileID
                    VALUES (@Username, @PasswordHash, @PasswordSalt)";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@Username", createAccountDTO.Username);
        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
        cmd.Parameters.AddWithValue("@PasswordSalt", salt);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Admin
            {
                AdminID = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                PasswordSalt = reader.GetString(3),
                ProfileID = reader.IsDBNull(4) ? null : reader.GetInt32(4)
            };
        }

        return null;
    }

    public async Task<bool> DeleteAdmin(int adminId)
    {
        using SqlConnection conn = new(_connectionString);
        string query = "DELETE FROM Admin WHERE AdminID = @AdminID";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@AdminID", adminId);

        await conn.OpenAsync();
        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<Admin?> GetAdmin(int adminId)
    {
        using SqlConnection conn = new(_connectionString);
        string query = "SELECT AdminID, Username, PasswordHash, PasswordSalt, ProfileID FROM Admin WHERE AdminID = @AdminID";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@AdminID", adminId);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Admin
            {
                AdminID = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                PasswordSalt = reader.GetString(3),
                ProfileID = reader.IsDBNull(4) ? null : reader.GetInt32(4)
            };
        }

        return null;
    }

    public async Task<bool> ValidateAdmin(string username, string password)
    {
        using SqlConnection conn = new(_connectionString);
        string query = "SELECT PasswordHash, PasswordSalt FROM Admin WHERE Username = @Username";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@Username", username);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            string storedHash = reader.GetString(0);
            string storedSalt = reader.GetString(1);
            return PasswordHasher.VerifyPassword(password, storedHash, storedSalt);
        }

        return false;
    }
}