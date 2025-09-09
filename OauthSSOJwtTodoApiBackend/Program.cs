using OauthSSOJwtTodoApiBackend.Configuration;
using OauthSSOJwtTodoApiBackend.Helpers;
using OauthSSOJwtTodoApiBackend.Middleware;
using OauthSSOJwtTodoApiBackend.Services;
using OauthSSOJwtTodoApiBackend.Services.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Configuration files
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

// Bind config sections to strongly-typed config
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<LinkedInOAuthSettings>(builder.Configuration.GetSection("LinkedIn"));
builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection("RateLimiting"));
builder.Services.Configure<AuthCookieSettings>(builder.Configuration.GetSection("AuthCookie"));
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

// logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Register helpers and services
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IUserSeeder, UserSeeder>();
builder.Services.AddScoped<ITodoSeeder, TodoSeeder>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<SeedService>();

// Logging: Currently using ILogger<T> directly in services.
// Uncomment the following line to use a custom LoggerService when external log sinks (e.g., Sentry, Seq) are introduced.
// builder.Services.AddScoped<ILoggerService, LoggerService>();

// Add MULTI-PROVIDER EF CORE SUPPORT using helper
builder.Services.AddMultiProviderDbContext(builder.Configuration);

// Add Rate Limiting using helper
builder.Services.AddRateLimiting(builder.Configuration);

// Register Authentication services using helper
builder.Services.AddAuthenticationServices(builder.Configuration);

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI setup
builder.Services.AddEndpointsApiExplorer();

// Register Swagger and OAuth2 setup using helper
builder.Services.AddSwaggerServices(builder.Configuration);

// CORS setup
builder.Services.AddCorsPolicy(builder.Configuration, builder.Environment);

var app = builder.Build();

// Use CORS policy
app.UseCors(CorsHelper.AllowCorsPolicyName); // Use the policy name defined in CorsHelper

// Middleware pipeline
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();

SwaggerUIHelper.ConfigureSwaggerUI(app);

// Developer exception page
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

// Production config check
LinkedInConfigValidator.ValidateLinkedInConfig(app);

// HTTPS + Routing + Auth
app.UseHttpsRedirection();
app.UseAuthentication();

// Custom middleware to support JWT from cookie
app.UseMiddleware<JwtCookieAuthMiddleware>();

app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

// Seed test data
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();
    await seedService.SeedAsync();
}

// While in development only - Diagnostic middleware for 404s.
app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();
    if (endpoint == null)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("No endpoint matched for request: {Method} {Path}", context.Request.Method, context.Request.Path);
    }
    await next();
});

// Optionally launch Swagger in browser
SwaggerBrowserLauncher.Launch(app.Configuration, app.Environment, app.Services, app.Logger);

app.Run();
