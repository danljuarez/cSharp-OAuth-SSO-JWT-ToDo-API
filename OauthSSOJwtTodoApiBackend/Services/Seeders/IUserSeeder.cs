using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Models.Entities;

namespace OauthSSOJwtTodoApiBackend.Services.Seeders
{
    public interface IUserSeeder
    {
        Task<IEnumerable<User>> SeedUsersAsync(TodoDbContext db);
    }
}