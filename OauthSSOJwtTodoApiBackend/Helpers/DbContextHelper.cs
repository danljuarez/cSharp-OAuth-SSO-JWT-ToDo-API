using Microsoft.EntityFrameworkCore;
using OauthSSOJwtTodoApiBackend.Data;

namespace OauthSSOJwtTodoApiBackend.Helpers;

/// <summary>
/// Provides extension methods for registering the application's DbContext
/// with multi-database provider support.
/// </summary>
public static class DbContextHelper
{
    /// <summary>
    /// Registers the <c>TodoDbContext</c> with the appropriate database provider
    /// based on the "Database:Provider" value from the configuration.
    /// </summary>
    /// <param name="services">
    /// The service collection to which the DbContext should be added.
    /// </param>
    /// <param name="configuration">
    /// The application's configuration instance, used to extract database provider and connection strings.
    /// </param>
    /// <remarks>
    /// Supported providers include: sqlite, sqlserver, postgres/postgresql, mysql, oracle, and inmemory (default).
    /// </remarks>
    public static void AddMultiProviderDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var dbConfig = configuration.GetSection("Database");
        var dbProvider = dbConfig["Provider"]?.ToLowerInvariant();

        services.AddDbContext<TodoDbContext>(options =>
        {
            var connStrings = dbConfig.GetSection("ConnectionStrings");

            switch (dbProvider)
            {
                case "sqlite":
                    options.UseSqlite(connStrings["Sqlite"]);
                    break;
                case "sqlserver":
                    options.UseSqlServer(connStrings["SqlServer"]);
                    break;
                case "postgres":
                case "postgresql":
                    options.UseNpgsql(connStrings["Postgres"]);
                    break;
                case "mysql":
                    options.UseMySql(connStrings["MySql"], ServerVersion.AutoDetect(connStrings["MySql"]));
                    break;
                case "oracle":
                    options.UseOracle(connStrings["Oracle"]);
                    break;
                case "inmemory":
                default:
                    options.UseInMemoryDatabase("TodoInMemoryDB");
                    break;
            }
        });
    }
}
