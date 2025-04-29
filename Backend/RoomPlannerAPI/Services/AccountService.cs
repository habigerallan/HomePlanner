using Microsoft.Data.SqlClient;
using RoomPlannerAPI.DTO;
using RoomPlannerAPI.Models;
using RoomPlannerAPI.Utilities;
using RoomPlannerAPI.Services.Interfaces;

namespace RoomPlannerAPI.Services;

public class AccountService(IConfiguration configuration) : IAccountService
{

    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is missing.");

    public async Task<Account?> Create(CreateAccountDTO createAccountDTO)
    {
        var (hashedPassword, salt) = PasswordHasher.HashPassword(createAccountDTO.Password);

        using SqlConnection conn = new(_connectionString);

        string query = @"
            INSERT INTO Account (Username, PasswordHash, PasswordSalt, ProfileID) 
            OUTPUT INSERTED.AccountID, INSERTED.Username, INSERTED.PasswordHash, INSERTED.PasswordSalt, INSERTED.ProfileID
            VALUES (@Username, @PasswordHash, @PasswordSalt, @ProfileID)";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@Username", createAccountDTO.Username);
        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
        cmd.Parameters.AddWithValue("@PasswordSalt", salt);
        cmd.Parameters.AddWithValue("@ProfileID", 0);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        var hasData = await reader.ReadAsync();

        if (!hasData)
        {
            return null;
        }

        Account account = new Account
        {
            AccountID = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            PasswordSalt = reader.GetString(3),
            ProfileID = reader.GetString(4)
        };

        return account;
    }

    public async Task<Account?> Read(ReadAccountDTO readAccountDTO)
    {
        using SqlConnection conn = new(_connectionString);

        string query = @"
            SELECT AccountID, Username, PasswordHash, PasswordSalt, ProfileID
            FROM Account 
            WHERE AccountID = @AccountID";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@AccountID", readAccountDTO.AccountId);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        var hasData = await reader.ReadAsync();

        if (!hasData)
        {
            return null;
        }

        Account account = new Account
        {
            AccountID = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            PasswordSalt = reader.GetString(3),
            ProfileID = reader.GetString(4)
        };

        return account;
    }

    public async Task<bool> Update(UpdateAccountDTO updateAccountDTO)
    {
        using SqlConnection conn = new(_connectionString);

        string query = @"
            UPDATE Account
            SET Username = @NUsername, ProfileID = @ProfileID
            OUTPUT INSERTED.AccountID, INSERTED.Username, INSERTED.PasswordHash, INSERTED.PasswordSalt, INSERTED.ProfileID
            WHERE AccountID = @AccountID";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@Username", updateAccountDTO.Username);
        cmd.Parameters.AddWithValue("@ProfileID", updateAccountDTO.ProfileID);

        await conn.OpenAsync();
        using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        var hasData = await reader.ReadAsync();

        if (!hasData)
        {
            return null;
        }

        Account account = new Account
        {
            AccountID = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            PasswordSalt = reader.GetString(3),
            ProfileID = reader.GetString(4)
        };

        return account;
    }

    public async Task<bool> Delete(int accountId)
    {
        using SqlConnection conn = new(_connectionString);
        string query = @"
            DELETE FROM Account 
            WHERE AccountID = @AccountID";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@AccountID", accountId);

        await conn.OpenAsync();
        int rowsAffected = await cmd.ExecuteNonQueryAsync();

        if (!rowsAffected)
        {
            return false
        }

        return true;
    }

    public async Task<int> Login(LoginAccountDTO loginAccountDTO)
    {
        using SqlConnection conn = new(_connectionString);
        string query = @"
            SELECT PasswordHash, PasswordSalt, AccountID 
            FROM Account
            WHERE Username = @Username";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@Username", loginAccountDTO.Username);

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
