using OauthSSOJwtTodoApiBackend.Data;
using OauthSSOJwtTodoApiBackend.Services.Seeders;

namespace OauthSSOJwtTodoApiBackend.Services;

public class SeedService
{
    private readonly TodoDbContext _db;
    private readonly IUserSeeder _userSeeder;
    private readonly ITodoSeeder _todoSeeder;
    private readonly ILogger<SeedService> _logger;

    public SeedService(
        TodoDbContext db,
        IUserSeeder userSeeder,
        ITodoSeeder todoSeeder,
        ILogger<SeedService> logger)
    {
        _db = db;
        _userSeeder = userSeeder;
        _todoSeeder = todoSeeder;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var seededUsers = await _userSeeder.SeedUsersAsync(_db);
        if (seededUsers.Any())
        {
            await _todoSeeder.SeedTodosAsync(_db, seededUsers);
            _logger.LogInformation($"Seeded {seededUsers.Count()} users and corresponding todos.");
        }
        else
        {
            _logger.LogInformation("Database already seeded.");
        }
    }
}
