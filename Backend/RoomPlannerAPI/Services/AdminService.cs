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

    public async Task<Admin?> Create(CreateAdminDTO createAdminDTO)
    {
        var (hashedPassword, salt) = PasswordHasher.HashPassword(createAdminDTO.Password);

        using SqlConnection conn = new(_connectionString);
        string query = @"
            INSERT INTO Admin (Username, PasswordHash, PasswordSalt) 
            OUTPUT INSERTED.AdminID, INSERTED.Username, INSERTED.PasswordHash, INSERTED.PasswordSalt, INSERTED.ProfileID
            VALUES (@Username, @PasswordHash, @PasswordSalt)";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@Username", createAdminDTO.Username);
        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
        cmd.Parameters.AddWithValue("@PasswordSalt", salt);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        var hasData = await reader.ReadAsync();

        if (!hasData)
        {
            return null;
        }

        Admin admin = new Admin
        {
            AdminID = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            PasswordSalt = reader.GetString(3),
            ProfileID = reader.GetString(4)
        };

        return admin;
    }

    public async Task<Admin?> Read(ReadAdminDTO readAdminDTO)
    {
        using SqlConnection conn = new(_connectionString);
        string query = @"
            SELECT AdminID, Username, PasswordHash, PasswordSalt, ProfileID
            FROM Admin
            WHERE AdminID = @AdminID";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@AdminID", readAdminDTO.AdminId);
        var hasData = await reader.ReadAsync();

        if (!hasData)
        {
            return null;
        }

        Admin admin = new Admin
        {
            AdminID = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            PasswordSalt = reader.GetString(3),
            ProfileID = reader.GetString(4)
        };

        return admin;
    }

    public async Task<Admin?> Update(UpdateAdminDTO updateAdminDTO) {
        return;
    }

    public async Task<bool> Delete(DeleteAdminDTO deleteAdminDTO)
    {
        using SqlConnection conn = new(_connectionString);
        string query = @"
            DELETE FROM Admin 
            WHERE AdminID = @AdminID";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@AdminID", deleteAdminDTO.AdminId);

        await conn.OpenAsync();
        int rowsAffected = await cmd.ExecuteNonQueryAsync();

        if (!rowsAffected)
        {
            return false
        }

        return true;
    }

    public async Task<bool> Login(LoginAdminDTO loginAdminDTO)
    {
        using SqlConnection conn = new(_connectionString);
        string query = @"
            SELECT PasswordHash, PasswordSalt, AccountID 
            FROM Account
            WHERE Username = @Username";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@Username", loginAdminDTO.Username);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        var hasData = await reader.ReadAsync();
        
        if (!hasData)
        {
            return null;
        }

        string storedHash = reader.GetString(0);
        string storedSalt = reader.GetString(1);
        int accountID = reader.GetInt32(2);

        if (!PasswordHasher.VerifyPassword(loginAccountDTO.Password, storedHash, storedSalt))
        {
            return 0;
        }

        return accountID;
    }
}
