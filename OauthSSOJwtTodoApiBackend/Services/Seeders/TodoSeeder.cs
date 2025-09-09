using Microsoft.EntityFrameworkCore;
using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Models.Entities;
using System.Text.Json;

namespace OauthSSOJwtTodoApiBackend.Services.Seeders;

public class TodoSeeder : ITodoSeeder
{
    private const string SEED_DATA_SOURCE = "resources/SeedData.json";

    public async Task SeedTodosAsync(TodoDbContext db, IEnumerable<User> users)
    {
        if (await db.Todos.AnyAsync())
            return;

        var json = await File.ReadAllTextAsync(SEED_DATA_SOURCE);
        using var doc = JsonDocument.Parse(json);
        var todosJson = doc.RootElement.GetProperty("todos");

        var todos = todosJson.EnumerateArray()
            .Select(t => new Todo
            {
                Title = t.GetProperty("title").GetString()!,
                Description = t.GetProperty("description").GetString()!,
                IsCompleted = t.GetProperty("completed").GetBoolean(),
                UserId = Guid.Parse(t.GetProperty("userId").GetString()!)
            })
            .ToList();

        await db.Todos.AddRangeAsync(todos);
        await db.SaveChangesAsync();
    }
}
