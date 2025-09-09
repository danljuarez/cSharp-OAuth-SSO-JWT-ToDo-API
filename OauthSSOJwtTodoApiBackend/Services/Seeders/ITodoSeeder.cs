using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Models.Entities;

namespace OauthSSOJwtTodoApiBackend.Services.Seeders
{
    public interface ITodoSeeder
    {
        Task SeedTodosAsync(TodoDbContext db, IEnumerable<User> users);
    }
}