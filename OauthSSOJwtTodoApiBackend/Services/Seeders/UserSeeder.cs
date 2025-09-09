using Microsoft.EntityFrameworkCore;
using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Helpers;
using OauthSSOJwtTodoApiBackend.Models.Entities;
using System.Text.Json;

namespace OauthSSOJwtTodoApiBackend.Services.Seeders;

public class UserSeeder : IUserSeeder
{
    private const string SEED_DATA_SOURCE = "resources/SeedData.json";

    public async Task<IEnumerable<User>> SeedUsersAsync(TodoDbContext db)
    {
        if (await db.Users.AnyAsync())
            return Enumerable.Empty<User>();

        var json = await File.ReadAllTextAsync(SEED_DATA_SOURCE);
        using var doc = JsonDocument.Parse(json);
        var usersJson = doc.RootElement.GetProperty("users");

        var users = usersJson.EnumerateArray().Select(u => new User
        {
            Id = Guid.Parse(u.GetProperty("id").GetString()!),
            Username = u.GetProperty("username").GetString()!,
            Email = u.GetProperty("email").GetString()!,
            HashedPassword = PasswordHasher.HashPassword(u.GetProperty("password").GetString()!),
            Role = u.GetProperty("role").GetString()!
        }).ToList();

        await db.Users.AddRangeAsync(users);
        await db.SaveChangesAsync();

        return users;
    }
}
